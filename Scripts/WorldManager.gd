@tool
extends Node3D

var SEED: int = 0

@export_range(0, 16, 2) var chunks_size: int = 4:
	set(value):
		chunks_size = value

@export_range(0, 64) var size: int = 1:
	set(value):
		size = value
		
@export_range(1, 8, 1) var resolution: int = 1:
	set(value):
		resolution = value
		
@export var use_seed: bool:
	set(value):
		use_seed = value
		randomize_seed()
		
@export_range(-1, 1, 0.1) var cutoff: float = 0.0:
	set(value):
		cutoff = value
		
@export var show_centers: bool:
	set(value):
		show_centers = value
		for child in get_children():
			if child is MeshInstance3D and child.name == "MeshInstanceCenters":
				child.visible = value
		
@export var show_grid: bool:
	set(value):
		show_grid = value
		for child in get_children():
			if child is MeshInstance3D and child.name == "MeshInstanceCubes":
				child.visible = value
				
@export var material: StandardMaterial3D

@export var noise_texture: Texture2D
		
# Syntax: @export_tool_button("Button Text", "IconName")
@export_tool_button("Generate World", "WorldEnvironment") var generate_world_action = generate_world

func generate_world():
	if not Engine.is_editor_hint():
		return
		
	if (use_seed):
		randomize_seed()
	
	remove_children()
	
	var start = - float(chunks_size) / 2
	var end = float(chunks_size) / 2
	var offset = size + 1
	
	for x in range(start, end):
		for z in range(start, end):
			generate(Vector3(offset * x, 0.0, offset * z))
			generate(Vector3(offset * x, offset, offset * z))
			#generate(Vector3(offset*x, offset*3, offset*z))
	
func generate(chunk_position: Vector3) -> void:
	print("Generating World...")
	
	# Instantiate noise
	print("Generating Noise...")
	
	# Create centers mesh
	print("Creating Mesh...")
	
	var img := noise_texture.get_image()
	
	# Create cubes mesh	
	var vertices: PackedVector3Array = PackedVector3Array()
	var normals: PackedVector3Array = PackedVector3Array()
	var indices: PackedInt32Array = PackedInt32Array()
	
	var start := float(size) / 2 * resolution * -1
	var end := (float(size) / 2 + 1) * resolution
	
	var vertex_count = 0;
	
	for x in range(start, end):
		for y in range(start, end):
			for z in range(start, end):
				var vertex := Vector3(float(x) / float(resolution), float(y) / float(resolution), float(z) / float(resolution))
					
				# Create marching cube vertices
				var cube_vertices: Array[Vector3] = create_cube_vertices(vertex)
				
				var cube_values: Array[float] = [
					get_noise_3d(cube_vertices[0].x + chunk_position.x, cube_vertices[0].y + chunk_position.y, cube_vertices[0].z + chunk_position.z, img),
					get_noise_3d(cube_vertices[1].x + chunk_position.x, cube_vertices[1].y + chunk_position.y, cube_vertices[1].z + chunk_position.z, img),
					get_noise_3d(cube_vertices[2].x + chunk_position.x, cube_vertices[2].y + chunk_position.y, cube_vertices[2].z + chunk_position.z, img),
					get_noise_3d(cube_vertices[3].x + chunk_position.x, cube_vertices[3].y + chunk_position.y, cube_vertices[3].z + chunk_position.z, img),
					get_noise_3d(cube_vertices[4].x + chunk_position.x, cube_vertices[4].y + chunk_position.y, cube_vertices[4].z + chunk_position.z, img),
					get_noise_3d(cube_vertices[5].x + chunk_position.x, cube_vertices[5].y + chunk_position.y, cube_vertices[5].z + chunk_position.z, img),
					get_noise_3d(cube_vertices[6].x + chunk_position.x, cube_vertices[6].y + chunk_position.y, cube_vertices[6].z + chunk_position.z, img),
					get_noise_3d(cube_vertices[7].x + chunk_position.x, cube_vertices[7].y + chunk_position.y, cube_vertices[7].z + chunk_position.z, img),
				]
				
				var lookup_index: int = get_lookup_index(cube_values)
				
				var triangles: Array = Constants.marching_triangules[lookup_index]
					
				for index in range(0, triangles.size(), 3):
					var point_1 = triangles[index]
					if point_1 == -1: continue
					
					var point_2 = triangles[index + 1]
					if point_2 == -1: continue
					
					var point_3 = triangles[index + 2]
					if point_3 == -1: continue
					
					var a0: int = Constants.cornerIndexAFromEdge[point_1]
					var b0: int = Constants.cornerIndexBFromEdge[point_1]
					
					var a1: int = Constants.cornerIndexAFromEdge[point_2]
					var b1: int = Constants.cornerIndexBFromEdge[point_2]
					
					var a2: int = Constants.cornerIndexAFromEdge[point_3]
					var b2: int = Constants.cornerIndexBFromEdge[point_3]
					
					var vertex1 = interpolate(cube_vertices[a0], cube_values[a0], cube_vertices[b0], cube_values[b0])
					var vertex2 = interpolate(cube_vertices[a1], cube_values[a1], cube_vertices[b1], cube_values[b1])
					var vertex3 = interpolate(cube_vertices[a2], cube_values[a2], cube_vertices[b2], cube_values[b2])
					
					# Create normal vector
					var vector_a := Vector3(
						vertex3.x - vertex1.x,
						vertex3.y - vertex1.y,
						vertex3.z - vertex1.z,
					)
					var vector_b := Vector3(
						vertex2.x - vertex1.x,
						vertex2.y - vertex1.y,
						vertex2.z - vertex1.z,
					)
					var vector_normal := Vector3(
						vector_a.y * vector_b.z - vector_a.z * vector_b.y,
						vector_a.z * vector_b.x - vector_a.x * vector_b.z,
						vector_a.x * vector_b.y - vector_a.y * vector_b.x
					)
					vertex_count += 1
										
					var base := vertices.size()
					vertices.append(vertex1)
					vertices.append(vertex2)
					vertices.append(vertex3)

					normals.append(vector_normal)
					normals.append(vector_normal)
					normals.append(vector_normal)

					indices.append(base)
					indices.append(base + 1)
					indices.append(base + 2)
					
					
	if vertex_count == 0:
		return
	var arrays := []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = vertices
	if normals.size() == vertices.size():
		arrays[Mesh.ARRAY_NORMAL] = normals
	if indices.size() > 0:
		arrays[Mesh.ARRAY_INDEX] = indices

	print("Creating Mesh Instance...")
	# Create triangles instance
	var mesh_instance_triangles = MeshInstance3D.new()
	mesh_instance_triangles.name = "Mesh" + str(chunk_position)
	mesh_instance_triangles.position = chunk_position
		
	print("Mixing...")
	var mesh := ArrayMesh.new()
	
	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	mesh.surface_set_material(0, material)
	
	mesh_instance_triangles.mesh = mesh
	add_child(mesh_instance_triangles)
	mesh_instance_triangles.owner = self
	
	add_trimesh_collision(self , mesh, Transform3D(Basis.IDENTITY, chunk_position))
	
	print("Generating Done")
	
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
	
# Clean up previous data
func remove_children():
	print("Removing Children...")
	var children := get_children()
	for child in children:
		remove_child(child)
		
	print("Removing Children Done")
	
func randomize_seed():
		SEED = randi()
		
func get_noise_3d(x: float, y: float, z: float, img: Image):
	var ton = img.get_pixel(int(x) + int(noise_texture.get_width() / 2), int(z) + int(noise_texture.get_width() / 2)).r * size
	return ton - y
		
func create_cube_vertices(base_vertex: Vector3) -> Array[Vector3]:
	return [
		Vector3(base_vertex.x, base_vertex.y, base_vertex.z),
		Vector3(base_vertex.x + 1, base_vertex.y, base_vertex.z),
		Vector3(base_vertex.x + 1, base_vertex.y, base_vertex.z + 1),
		Vector3(base_vertex.x, base_vertex.y, base_vertex.z + 1),
		Vector3(base_vertex.x, base_vertex.y + 1, base_vertex.z),
		Vector3(base_vertex.x + 1, base_vertex.y + 1, base_vertex.z),
		Vector3(base_vertex.x + 1, base_vertex.y + 1, base_vertex.z + 1),
		Vector3(base_vertex.x, base_vertex.y + 1, base_vertex.z + 1),
	]

func get_lookup_index(cubes_values: Array[float]) -> int:
	var cube_index: int = 0
	if cubes_values[0] < cutoff: cube_index |= 1
	if cubes_values[1] < cutoff: cube_index |= 2
	if cubes_values[2] < cutoff: cube_index |= 4
	if cubes_values[3] < cutoff: cube_index |= 8
	if cubes_values[4] < cutoff: cube_index |= 16
	if cubes_values[5] < cutoff: cube_index |= 32
	if cubes_values[6] < cutoff: cube_index |= 64
	if cubes_values[7] < cutoff: cube_index |= 128
	return cube_index

func interpolate(vertex1: Vector3, value1: float, vertex2: Vector3, value2: float) -> Vector3:
	return vertex1 + (cutoff - value1) * (vertex2 - vertex1) / (value2 - value1)
