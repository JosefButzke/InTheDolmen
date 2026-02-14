@tool
extends Node3D

@export var input_texture: Texture2D
@export var output_texture: Texture2D

@export_range(0, 8) var _octaves: int = 5:
	set(value):
		_octaves = value

@export_range(0, 16, 0.1) var _lacunarity: float = 2.0:
	set(value):
		_lacunarity = value
		
@export_range(0, 1, 0.1) var _gain: float = 0.5:
	set(value):
		_gain = value
		
@export_range(0, 16, 0.1) var _scale: float = 4.0:
	set(value):
		_scale = value

@export_tool_button("Run Compute", "Shader") var run_compute_action = run_compute

func run_compute():
	if input_texture == null:
		push_warning("Assign an input texture first.")
		return

	var rd := RenderingServer.create_local_rendering_device()

	# --- Load compute shader ---
	var shader_file := load("res://Shaders/compute.glsl")
	var shader := rd.shader_create_from_spirv(shader_file.get_spirv())
	var pipeline := rd.compute_pipeline_create(shader)

	# --- Prepare input texture ---
	var img: Image = input_texture.get_image()
	img.convert(Image.FORMAT_RGBA8)

	var w := img.get_width()
	var h := img.get_height()

	var fmt_in := RDTextureFormat.new()
	fmt_in.width = w
	fmt_in.height = h
	fmt_in.format = RenderingDevice.DATA_FORMAT_R8G8B8A8_UNORM
	fmt_in.usage_bits = RenderingDevice.TEXTURE_USAGE_SAMPLING_BIT \
		| RenderingDevice.TEXTURE_USAGE_CAN_UPDATE_BIT

	var tex_in := rd.texture_create(fmt_in, RDTextureView.new(), [img.get_data()])

	var sampler_state := RDSamplerState.new()
	sampler_state.unnormalized_uvw = true
	var sampler := rd.sampler_create(sampler_state)

	var in_uniform := RDUniform.new()
	in_uniform.uniform_type = RenderingDevice.UNIFORM_TYPE_SAMPLER_WITH_TEXTURE
	in_uniform.binding = 1
	in_uniform.add_id(sampler)
	in_uniform.add_id(tex_in)

	# --- Output storage texture ---
	var fmt_out := RDTextureFormat.new()
	fmt_out.width = w
	fmt_out.height = h
	fmt_out.format = RenderingDevice.DATA_FORMAT_R8G8B8A8_UNORM
	fmt_out.usage_bits = RenderingDevice.TEXTURE_USAGE_STORAGE_BIT \
		| RenderingDevice.TEXTURE_USAGE_CAN_COPY_FROM_BIT

	var tex_out := rd.texture_create(fmt_out, RDTextureView.new(), [])

	var out_uniform := RDUniform.new()
	out_uniform.uniform_type = RenderingDevice.UNIFORM_TYPE_IMAGE
	out_uniform.binding = 0
	out_uniform.add_id(tex_out)

	# --- Bind uniforms ---
	var params = PackedFloat32Array([
		float(_octaves),
		float(_lacunarity),
		float(_gain),
		float(_scale)
	]).to_byte_array()
	var param_buffer := rd.uniform_buffer_create(params.size(), params)
	var param_uniform := RDUniform.new()
	param_uniform.uniform_type = RenderingDevice.UNIFORM_TYPE_UNIFORM_BUFFER
	param_uniform.binding = 2
	param_uniform.add_id(param_buffer)

	var uniform_set := rd.uniform_set_create(
		[out_uniform, in_uniform, param_uniform],
		shader,
		0
	)

	# --- Dispatch compute ---
	var compute_list := rd.compute_list_begin()
	rd.compute_list_bind_compute_pipeline(compute_list, pipeline)
	rd.compute_list_bind_uniform_set(compute_list, uniform_set, 0)

	var gx := int(ceil(float(w) / 8.0))
	var gy := int(ceil(float(h) / 8.0))
	rd.compute_list_dispatch(compute_list, gx, gy, 1)

	rd.compute_list_end()
	rd.submit()
	rd.sync ()

	# --- Read back result ---
	var bytes := rd.texture_get_data(tex_out, 0)
	var out_img := Image.create_from_data(w, h, false, Image.FORMAT_RGBA8, bytes)

	output_texture = ImageTexture.create_from_image(out_img)
	var img_generated: Image = output_texture.get_image()
	img_generated.save_png("res://saved_texture.png")
	print("Compute shader finished.")
