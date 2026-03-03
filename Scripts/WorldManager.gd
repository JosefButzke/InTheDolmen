@tool

extends Node3D
class_name MarchCubesCompute

@export_range(1, 32) var chunksFromPlayer: int = 1:
	set(value):
		chunksFromPlayer = value

@export_range(8, 4096, 8) var chunkWidth: int = 32:
	set(value):
		chunkWidth = value
		
@export_range(8, 256, 8) var chunkHeight: int = 32:
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

@export_tool_button("Run Compute", "Shader") var run_compute_action = generate_world

const VERTICES_SHADER := preload("res://Shaders/vertices.glsl")
const MC_SHADER := preload("res://Shaders/march_cube.glsl")

var _rd: RenderingDevice
var _shader_vertices: RID
var _shader_march_cube: RID
var _pipeline: RID
var _pipelineMC: RID

func _ready():
	_rd = RenderingServer.create_local_rendering_device()
	_shader_vertices = _rd.shader_create_from_spirv(VERTICES_SHADER.get_spirv())
	_shader_march_cube = _rd.shader_create_from_spirv(MC_SHADER.get_spirv())
	_pipeline = _rd.compute_pipeline_create(_shader_vertices)
	_pipelineMC = _rd.compute_pipeline_create(_shader_march_cube)

func generate_world():
	remove_children()

	for x in range(-chunksFromPlayer, chunksFromPlayer):
			for z in range(-chunksFromPlayer, chunksFromPlayer):
				generate_chunk(Vector3(x * (chunkWidth - 1), -chunkHeight / 2.0, z * (chunkWidth - 1)))


func generate_chunk(chunk_position: Vector3):
	var rd := _rd
	var shader_vertices := _shader_vertices
	var shader_march_cube := _shader_march_cube
	var pipeline := _pipeline
	var pipelineMC := _pipelineMC

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
		1.0, # t1
		1.0 # t2
	]).to_byte_array()
	var bufferChunkParams := rd.uniform_buffer_create(chunkParams.size(), chunkParams)
	
	var uniformChunkParams := RDUniform.new()
	uniformChunkParams.uniform_type = RenderingDevice.UNIFORM_TYPE_UNIFORM_BUFFER
	uniformChunkParams.binding = 2 # this needs to match the "binding" in our shader file
	uniformChunkParams.add_id(bufferChunkParams)

		#Buffer chunk offset
	var chunkOffset = PackedFloat32Array([
		float(chunk_position.x),
		float(chunk_position.y),
		float(chunk_position.z),
		1.0 # t2
	]).to_byte_array()
	var bufferChunkOffset := rd.uniform_buffer_create(chunkOffset.size(), chunkOffset)
	
	var uniformChunkOffset := RDUniform.new()
	uniformChunkOffset.uniform_type = RenderingDevice.UNIFORM_TYPE_UNIFORM_BUFFER
	uniformChunkOffset.binding = 3 # this needs to match the "binding" in our shader file
	uniformChunkOffset.add_id(bufferChunkOffset)
	
	var uniform_set := rd.uniform_set_create([uniformVertices, uniformNoiseParams, uniformChunkParams, uniformChunkOffset], shader_vertices, 0) # the last parameter (the 0) needs to match the "set" in our shader file
	
	# Create a compute pipeline

	var compute_list := rd.compute_list_begin()
	rd.compute_list_bind_compute_pipeline(compute_list, pipeline)
	rd.compute_list_bind_uniform_set(compute_list, uniform_set, 0)
	rd.compute_list_dispatch(compute_list, int(chunkWidth / 8.0), int(chunkHeight / 8.0), int(chunkWidth / 8.0))
	
	rd.compute_list_add_barrier(compute_list)

	var uniformVerticesIn := RDUniform.new()
	uniformVerticesIn.uniform_type = RenderingDevice.UNIFORM_TYPE_STORAGE_BUFFER
	uniformVerticesIn.binding = 0 # this needs to match the "binding" in our shader file
	uniformVerticesIn.add_id(bufferVertices)

	var bufferVerticesOut: RID = rd.storage_buffer_create(chunkWidth * chunkWidth * chunkHeight * floatBytes * vector4Items * 5)
	
	var uniformVerticesOut := RDUniform.new()
	uniformVerticesOut.uniform_type = RenderingDevice.UNIFORM_TYPE_STORAGE_BUFFER
	uniformVerticesOut.binding = 1 # this needs to match the "binding" in our shader file
	uniformVerticesOut.add_id(bufferVerticesOut)
	
	#Buffer chunk params
	var chunkParamsMC = PackedFloat32Array([
		float(chunkWidth),
		float(chunkHeight),
		1.0, # t1
		1.0 # t2
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

	# Create a compute pipeline
	rd.compute_list_bind_compute_pipeline(compute_list, pipelineMC)
	rd.compute_list_bind_uniform_set(compute_list, uniform_set_march_cube, 0)
	rd.compute_list_dispatch(compute_list, int((chunkWidth) / 8.0), int(chunkHeight / 8.0), int((chunkWidth) / 8.0))
	rd.compute_list_end()

	# Submit to GPU and wait for sync
	rd.submit()
	rd.sync ()
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

	var mesh := ArrayMesh.new()

	# --- Build mesh arrays ---
	var arrays := []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = triangles
	arrays[Mesh.ARRAY_NORMAL] = normals
	
	var mesh_instance_triangles = MeshInstance3D.new()
	mesh_instance_triangles.name = "Mesh" + str(chunk_position)

	mesh_instance_triangles.position = chunk_position

	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	mesh.surface_set_material(0, material)
	
	mesh_instance_triangles.mesh = mesh
	add_child(mesh_instance_triangles)
	mesh_instance_triangles.owner = self
	add_trimesh_collision(self , mesh, mesh_instance_triangles.position)
	
func add_trimesh_collision(parent: Node3D, tri_mesh: Mesh, p: Vector3) -> void:
	# Static body to hold the collision
	var body := StaticBody3D.new()
	body.name = "TrianglesBody"
	body.position = p
	parent.add_child(body)

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
	var children := get_children()
	for child in children:
		remove_child(child)
