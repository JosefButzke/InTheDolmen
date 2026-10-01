extends MultiMeshInstance3D

const NUM_GRASS := 512 * 1024
const GRASS_PATCH_SIZE := 96.0
const PLANET_RADIUS := 1024.0 # must match PLANET_RADIUS in shaders/planet_noise.glslinc
const FLOATS_PER_INSTANCE := 16 # TRANSFORM_3D (12) + custom data (4)
const PLACEMENT_SHADER := preload("res://shaders/grass_placement.glsl")

@export var material: ShaderMaterial
## Planet terrain (MarchCubesCompute). Grass is placed on its surface, in its local space.
@export var planet: Node3D
## Fallback grid resolution when the planet node doesn't expose `resolution`.
@export var terrain_resolution: float = 4.0
@export var grass_seed: int = 0
## Random height range added on top of grass_min_height.
@export var grass_height: float = 5.0
@export var grass_min_height: float = 1.0

func build_grass_multimesh(base_mesh: Mesh, num_grass: int) -> MultiMesh:
	var mm := MultiMesh.new()
	mm.mesh = base_mesh
	mm.transform_format = MultiMesh.TRANSFORM_3D

	# Optional but common for grass: allow per-instance data in shader via INSTANCE_CUSTOM.
	mm.use_custom_data = true  # must be set before instance_count in Godot 4.x :contentReference[oaicite:1]{index=1}

	mm.instance_count = num_grass  # <- like geo.instanceCount = NUM_GRASS :contentReference[oaicite:2]{index=2}
	return mm

func attach_multimesh(mm: MultiMesh) -> MultiMeshInstance3D:
	var inst := MultiMeshInstance3D.new()
	inst.multimesh = mm

	add_child(inst)
	return inst

func _build_base_mesh() -> ArrayMesh:
	var mesh := ArrayMesh.new()

	var vertices := PackedVector3Array([
		Vector3(-0.5, 0.0, 0.0),
		Vector3(0.5,  0.0, 0.0),
		Vector3(-0.5, 1.0, 0.0),
		Vector3(0.5,  1.0, 0.0),
		Vector3(-0.5, 2.0, 0.0),
		Vector3(0.5,  2.0, 0.0),
		Vector3(-0.5, 3.0, 0.0),
		Vector3(0.5,  3.0, 0.0),
		Vector3(-0.5, 4.0, 0.0),
		Vector3(0.5,  4.0, 0.0),
		Vector3(-0.5, 5.0, 0.0),
		Vector3(0.5,  5.0, 0.0)
	])

	var indices := PackedInt32Array([
		0, 1, 2,
		2, 1, 3,

		2, 3, 4,
		4, 3, 5,

		4, 5, 6,
		6, 5, 7,

		6, 7, 8,
		8, 7, 9,

		8, 9, 10,
		10, 9, 11
	])

	# --- build surface ---
	var arrays := []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = vertices
	arrays[Mesh.ARRAY_INDEX] = indices

	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	return mesh


func _ready():
	var base_mesh := _build_base_mesh()
	material.set_shader_parameter("grass_height", grass_height)
	material.set_shader_parameter("grass_min_height", grass_min_height)
	base_mesh.surface_set_material(0, material)
	var mm := MultiMesh.new()
	mm.mesh = base_mesh
	mm.transform_format = MultiMesh.TRANSFORM_3D
	mm.use_custom_data = true
	mm.instance_count = NUM_GRASS

	# Assign to this node (no add_child needed)
	multimesh = mm

	if planet == null:
		planet = get_node_or_null("../Planet") as Node3D

	# Planet space -> this node's local space (the terrain noise is evaluated in planet space)
	var planet_xform := planet.global_transform if planet else Transform3D.IDENTITY
	var planet_to_local := global_transform.affine_inverse() * planet_xform

	# Patch is centered under this node, projected onto the planet
	var center_dir := (planet_to_local.affine_inverse() * Vector3.ZERO).normalized()
	if center_dir == Vector3.ZERO:
		center_dir = Vector3.UP

	mm.buffer = _place_on_planet(center_dir, planet_to_local)

	# Override bounds for culling (AABB, not sphere)
	var center_local := planet_to_local * (center_dir * PLANET_RADIUS)
	var extent := GRASS_PATCH_SIZE + 32.0
	custom_aabb = AABB(center_local - Vector3.ONE * extent, Vector3.ONE * extent * 2.0)

func _place_on_planet(center_dir: Vector3, planet_to_local: Transform3D) -> PackedFloat32Array:
	var resolution := terrain_resolution
	if planet and planet.get("resolution") != null:
		resolution = float(planet.get("resolution"))
	var iso_level := MarchCubesCompute.ISO_LEVEL

	var ref := Vector3.UP if absf(center_dir.y) < 0.99 else Vector3.RIGHT
	var tangent := center_dir.cross(ref).normalized()
	var bitangent := tangent.cross(center_dir)

	var b := planet_to_local.basis
	var o := planet_to_local.origin
	var params := PackedFloat32Array([
		center_dir.x, center_dir.y, center_dir.z, GRASS_PATCH_SIZE,
		tangent.x, tangent.y, tangent.z, resolution,
		bitangent.x, bitangent.y, bitangent.z, iso_level,
		float(NUM_GRASS), float(grass_seed), 0.0, 0.0,
		# mat4, column-major
		b.x.x, b.x.y, b.x.z, 0.0,
		b.y.x, b.y.y, b.y.z, 0.0,
		b.z.x, b.z.y, b.z.z, 0.0,
		o.x, o.y, o.z, 1.0,
	]).to_byte_array()

	var rd := RenderingServer.create_local_rendering_device()
	var shader := rd.shader_create_from_spirv(PLACEMENT_SHADER.get_spirv())
	var pipeline := rd.compute_pipeline_create(shader)

	var buffer_instances := rd.storage_buffer_create(NUM_GRASS * FLOATS_PER_INSTANCE * 4)
	var uniform_instances := RDUniform.new()
	uniform_instances.uniform_type = RenderingDevice.UNIFORM_TYPE_STORAGE_BUFFER
	uniform_instances.binding = 0
	uniform_instances.add_id(buffer_instances)

	var buffer_params := rd.uniform_buffer_create(params.size(), params)
	var uniform_params := RDUniform.new()
	uniform_params.uniform_type = RenderingDevice.UNIFORM_TYPE_UNIFORM_BUFFER
	uniform_params.binding = 1
	uniform_params.add_id(buffer_params)

	var uniform_set := rd.uniform_set_create([uniform_instances, uniform_params], shader, 0)

	var compute_list := rd.compute_list_begin()
	rd.compute_list_bind_compute_pipeline(compute_list, pipeline)
	rd.compute_list_bind_uniform_set(compute_list, uniform_set, 0)
	rd.compute_list_dispatch(compute_list, ceili(NUM_GRASS / 64.0), 1, 1)
	rd.compute_list_end()
	rd.submit()
	rd.sync()

	var data := rd.buffer_get_data(buffer_instances).to_float32_array()

	rd.free_rid(uniform_set)
	rd.free_rid(buffer_params)
	rd.free_rid(buffer_instances)
	rd.free_rid(pipeline)
	rd.free_rid(shader)
	rd.free()
	return data
