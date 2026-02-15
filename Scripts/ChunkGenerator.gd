@tool

extends Node3D
class_name MarchCubesCompute

@export_range(8, 256, 8) var chunkWidth: int = 32:
	set(value):
		chunkWidth = value
		
@export_range(8, 64, 8) var chunkHeight: int = 32:
	set(value):
		chunkHeight = value

@export_range(0, 8) var _octaves: int = 5:
	set(value):
		_octaves = value

@export_range(0, 16, 0.1) var _lacunarity: float = 2.0:
	set(value):
		_lacunarity = value
		
@export_range(0, 1, 0.1) var _gain: float = 0.3:
	set(value):
		_gain = value
		
@export_range(16, 256, 16) var _scale: float = 16:
	set(value):
		_scale = value
		
@export var material: StandardMaterial3D

@export_tool_button("Run Compute", "Shader") var run_compute_action = run_compute

const VERTICES_SHADER := preload("res://Shaders/vertices.glsl")
const MC_SHADER := preload("res://Shaders/march_cube.glsl")

func run_compute():
	print("---------- Start vertices compute shader ---------- ", Time.get_time_string_from_system())
	remove_children()
	print("start", Time.get_time_string_from_system())
	# Create a local rendering device.
	var rd := RenderingServer.create_local_rendering_device()
	print("p1", Time.get_time_string_from_system())
	# Load GLSL shader
	var shader_vertices_file := VERTICES_SHADER
	var shader_vertices_spirv: RDShaderSPIRV = shader_vertices_file.get_spirv()
	var shader_vertices := rd.shader_create_from_spirv(shader_vertices_spirv)
	print("p2", Time.get_time_string_from_system())
	var shader_march_cube_file := MC_SHADER
	print("p2.1", Time.get_time_string_from_system())
	var shader_march_cube_spirv: RDShaderSPIRV = shader_march_cube_file.get_spirv()
	print("p2.2", Time.get_time_string_from_system())
	var shader_march_cube := rd.shader_create_from_spirv(shader_march_cube_spirv)
	print("p3", Time.get_time_string_from_system())
	var pipeline := rd.compute_pipeline_create(shader_vertices)
	var pipelineMC := rd.compute_pipeline_create(shader_march_cube)
	print("end pipeline", Time.get_time_string_from_system())
	# Buffer output to vertices
	# Each float has 4 bytes (32 bit)
	var floatBytes = 4
	var vector4Items = 4
	var bufferVertices: RID = rd.storage_buffer_create(chunkWidth * chunkWidth * chunkHeight * floatBytes * vector4Items)
	
	var uniformVertices := RDUniform.new()
	uniformVertices.uniform_type = RenderingDevice.UNIFORM_TYPE_STORAGE_BUFFER
	uniformVertices.binding = 0 # this needs to match the "binding" in our shader file
	uniformVertices.add_id(bufferVertices)

	#Buffer noise params
	var noiseParams = PackedFloat32Array([
		float(_octaves),
		float(_lacunarity),
		float(_gain),
		float(_scale)
	]).to_byte_array()
	var bufferNoiseParams := rd.uniform_buffer_create(noiseParams.size(), noiseParams)
	
	var uniformNoiseParams := RDUniform.new()
	uniformNoiseParams.uniform_type = RenderingDevice.UNIFORM_TYPE_UNIFORM_BUFFER
	uniformNoiseParams.binding = 1 # this needs to match the "binding" in our shader file
	uniformNoiseParams.add_id(bufferNoiseParams)
	
	#Buffer chunk params
	var chunkParams = PackedFloat32Array([
		float(chunkWidth),
		float(chunkHeight),
		1, # t1
		1 # t2
	]).to_byte_array()
	var bufferChunkParams := rd.uniform_buffer_create(chunkParams.size(), chunkParams)
	
	var uniformChunkParams := RDUniform.new()
	uniformChunkParams.uniform_type = RenderingDevice.UNIFORM_TYPE_UNIFORM_BUFFER
	uniformChunkParams.binding = 2 # this needs to match the "binding" in our shader file
	uniformChunkParams.add_id(bufferChunkParams)
	
	var uniform_set := rd.uniform_set_create([uniformVertices, uniformNoiseParams, uniformChunkParams], shader_vertices, 0) # the last parameter (the 0) needs to match the "set" in our shader file
	print("end uni vert", Time.get_time_string_from_system())
	# Create a compute pipeline

	var compute_list := rd.compute_list_begin()
	rd.compute_list_bind_compute_pipeline(compute_list, pipeline)
	rd.compute_list_bind_uniform_set(compute_list, uniform_set, 0)
	rd.compute_list_dispatch(compute_list, int(chunkWidth / 8.0), int(chunkHeight / 8.0), int(chunkWidth / 8.0))
	
	rd.compute_list_add_barrier(compute_list)
	print("end pipe vert", Time.get_time_string_from_system())	

	print("start uni mc", Time.get_time_string_from_system())
	var uniformVerticesIn := RDUniform.new()
	uniformVerticesIn.uniform_type = RenderingDevice.UNIFORM_TYPE_STORAGE_BUFFER
	uniformVerticesIn.binding = 0 # this needs to match the "binding" in our shader file
	uniformVerticesIn.add_id(bufferVertices)
	print(444, Time.get_time_string_from_system())
	var bufferVerticesOut: RID = rd.storage_buffer_create(chunkWidth * chunkWidth * chunkHeight * floatBytes * vector4Items * 5)
	
	var uniformVerticesOut := RDUniform.new()
	uniformVerticesOut.uniform_type = RenderingDevice.UNIFORM_TYPE_STORAGE_BUFFER
	uniformVerticesOut.binding = 1 # this needs to match the "binding" in our shader file
	uniformVerticesOut.add_id(bufferVerticesOut)
	
	#Buffer chunk params
	var chunkParamsMC = PackedFloat32Array([
		float(chunkWidth),
		float(chunkHeight),
		1, # t1
		1 # t2
	]).to_byte_array()
	var bufferChunkParamsMC := rd.uniform_buffer_create(chunkParamsMC.size(), chunkParamsMC)
	
	var uniformChunkParamsMarchCube := RDUniform.new()
	uniformChunkParamsMarchCube.uniform_type = RenderingDevice.UNIFORM_TYPE_UNIFORM_BUFFER
	uniformChunkParamsMarchCube.binding = 2 # this needs to match the "binding" in our shader file
	uniformChunkParamsMarchCube.add_id(bufferChunkParamsMC)
	
	var counterInit := PackedInt32Array([0]).to_byte_array()
	var counterBuffer := rd.storage_buffer_create(4, counterInit)
	
	var uniformCounter := RDUniform.new()
	uniformCounter.uniform_type = RenderingDevice.UNIFORM_TYPE_STORAGE_BUFFER
	uniformCounter.binding = 3 # this needs to match the "binding" in our shader file
	uniformCounter.add_id(counterBuffer)
	
	var bufferNormalsOut: RID = rd.storage_buffer_create(chunkWidth * chunkWidth * chunkHeight * floatBytes * vector4Items * 5)
	
	var uniformNormalsOut := RDUniform.new()
	uniformNormalsOut.uniform_type = RenderingDevice.UNIFORM_TYPE_STORAGE_BUFFER
	uniformNormalsOut.binding = 4 # this needs to match the "binding" in our shader file
	uniformNormalsOut.add_id(bufferNormalsOut)
	
	var uniform_set_march_cube := rd.uniform_set_create([uniformVerticesIn, uniformVerticesOut, uniformChunkParamsMarchCube, uniformCounter, uniformNormalsOut], shader_march_cube, 0) # the last parameter (the 0) needs to match the "set" in our shader file
	print("end uni mc", Time.get_time_string_from_system())
	# Create a compute pipeline
	
	print("start pipe mc", Time.get_time_string_from_system())
	rd.compute_list_bind_compute_pipeline(compute_list, pipelineMC)
	rd.compute_list_bind_uniform_set(compute_list, uniform_set_march_cube, 0)
	rd.compute_list_dispatch(compute_list, int((chunkWidth) / 8.0), int(chunkHeight / 8.0), int((chunkWidth) / 8.0))
	rd.compute_list_end()
	print(3, Time.get_time_string_from_system())
	# Submit to GPU and wait for sync
	print("end pipe mc", Time.get_time_string_from_system())
	print("start sync", Time.get_time_string_from_system())
	rd.submit()
	rd.sync ()
	print("end sync", Time.get_time_string_from_system())
	# Read back the data from the buffer

	var output_vertices_bytes = rd.buffer_get_data(bufferVerticesOut)
	var output_normals_bytes = rd.buffer_get_data(bufferNormalsOut)
	var output_vertices_vec4 = output_vertices_bytes.to_vector4_array()
	var output_normals_vec4 = output_normals_bytes.to_vector4_array()
	var triangles: PackedVector3Array = PackedVector3Array()
	var normals: PackedVector3Array = PackedVector3Array()
		
	var counter_bytes = rd.buffer_get_data(counterBuffer)
	var vertex_count = counter_bytes.to_int32_array()[0]

	for i in vertex_count:
		triangles.append(Vector3(
			output_vertices_vec4[i].x,
			output_vertices_vec4[i].y,
			output_vertices_vec4[i].z
		))
		normals.append(Vector3(
			output_normals_vec4[i].x,
			output_normals_vec4[i].y,
			output_normals_vec4[i].z
		))
		
		
	print(5, Time.get_time_string_from_system())
	print("Triangle size/ ", triangles.size(), " : ", vertex_count)

	var mesh := ArrayMesh.new()

	# --- Build mesh arrays ---
	var arrays := []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = triangles
	arrays[Mesh.ARRAY_NORMAL] = normals
	
	var mesh_instance_triangles = MeshInstance3D.new()
	mesh_instance_triangles.name = "Mesh" + str(Vector3(0, 0, 0))

	mesh_instance_triangles.position = Vector3(-chunkWidth / 2.0, -chunkHeight / 2.0, -chunkWidth / 2.0)

	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	mesh.surface_set_material(0, material)
	
	mesh_instance_triangles.mesh = mesh
	add_child(mesh_instance_triangles)
	mesh_instance_triangles.owner = self
	print(6, Time.get_time_string_from_system())
	add_trimesh_collision(self , mesh, Transform3D(Basis.IDENTITY, mesh_instance_triangles.position))
	print(7, Time.get_time_string_from_system())
	
func add_trimesh_collision(parent: Node3D, tri_mesh: Mesh, xform: Transform3D) -> void:
	# Static body to hold the collision
	var body := StaticBody3D.new()
	body.name = "TrianglesBody"
	parent.add_child(body)
	body.transform = xform

	# Create concave collision from the mesh triangles
	var shape := ConcavePolygonShape3D.new()
	var arrays := tri_mesh.surface_get_arrays(0)
	# Arrays[Mesh.ARRAY_VERTEX] is a PackedVector3Array for non-indexed triangle meshes
	shape.data = arrays[Mesh.ARRAY_VERTEX]

	var col := CollisionShape3D.new()
	col.name = "TrianglesCollision"
	col.shape = shape
	body.add_child(col)
	
	# Ensure ownership so it gets saved
	body.owner = parent
	col.owner = parent
	

func remove_children():
	print("Removing Children...")
	var children := get_children()
	for child in children:
		remove_child(child)
		
	print("Removing Children Done")
