@tool
extends Node3D

var SEED: int = 0

@export_range(0, 64) var size: int = 1:
	set(value):
		size = value
		
@export_range(1, 8, 1) var resolution: int = 1:
	set(value):
		resolution = value
		
@export var randomize: bool:
	set(value):
		randomize = value
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
		
# Syntax: @export_tool_button("Button Text", "IconName")
@export_tool_button("Generate World", "WorldEnvironment") var generate_world_action = generate_world


func _ready():
	generate_world()

func generate_world():
	if(randomize):
		randomize_seed()
		
	remove_children()
	generate(Vector3(0, 0, 0))
	generate(Vector3((size+1), 0, 0))
	generate(Vector3((size+1)*2, 0, 0))
	generate(Vector3((size+1)*3, 0, 0))
	
	generate(Vector3(0, 0, (size+1)))
	generate(Vector3((size+1), 0, (size+1)))
	generate(Vector3((size+1)*2, 0, (size+1)))
	generate(Vector3((size+1)*3, 0, (size+1)))
	
	generate(Vector3(0, 0, (size+1)*2))
	generate(Vector3((size+1), 0, (size+1)*2))
	generate(Vector3((size+1)*2, 0, (size+1)*2))
	generate(Vector3((size+1)*3, 0, (size+1)*2))
	
	generate(Vector3(0, 0, (size+1)*3))
	generate(Vector3((size+1), 0, (size+1)*3))
	generate(Vector3((size+1)*2, 0, (size+1)*3))
	generate(Vector3((size+1)*3, 0, (size+1)*3))
	
	
func generate(position: Vector3) -> void:
	print("Generating World...")
	
	# Instantiate noise
	print("Generating Noise...")
	var noise = FastNoiseLite.new()
	noise.seed = SEED
	
	# Create centers mesh
	print("Creating Mesh...")
	
	# Create cubes mesh
	var mesh_triangles = ImmediateMesh.new()
	mesh_triangles.surface_begin(Mesh.PRIMITIVE_TRIANGLES)
	
	var start := size/2 * resolution * -1
	var end := (size/2 + 1) * resolution
	
	for x in range(start, end):
		for y in range(start, end):
			for z in range(start, end):
				var center := Vector3(float(x)/float(resolution), float(y)/float(resolution), float(z)/float(resolution))
					
				# Create marching cube vertices
				var cube_vertices: Array[Vector3] = create_cube_vertices(center)
				
				# Get the scalar values at the corners of the current cube
				var cube_values: Array[float] = get_cube_values(noise, cube_vertices, position)
									
				var lookup_index: int = get_lookup_index(cube_values)
				
				var triangles: Array = Constants.marching_triangules[lookup_index]
				
				var color: = Color(
					float(center.x + size) / float(size * 2),
					float(center.y + size) / float(size * 2),
					float(center.z + size) / float(size * 2)
				)
					
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
					
					mesh_triangles.surface_set_color(color)
					mesh_triangles.surface_set_normal(vector_normal)
					mesh_triangles.surface_add_vertex(vertex1)
					mesh_triangles.surface_add_vertex(vertex2)
					mesh_triangles.surface_add_vertex(vertex3)					
				
	mesh_triangles.surface_end()
	
	print("Creating Mesh Instance...")
	
	# Create triangles instance
	var mesh_instance_triangles = MeshInstance3D.new()
	add_child(mesh_instance_triangles)
	mesh_instance_triangles.global_position = position
		
	print("Mixing...")	
	mesh_triangles.surface_set_material(0, material)
	mesh_instance_triangles.mesh = mesh_triangles
	add_trimesh_collision(self, mesh_triangles, Transform3D(Basis.IDENTITY, position))

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
	
# Clean up previous data
func remove_children():
	print("Removing Children...")
	var children := get_children()
	for child in children:
		remove_child(child)
		
	print("Removing Children Done")
	
func randomize_seed():
		SEED = randi()
		
func create_cube_vertices(position: Vector3) -> Array[Vector3]: 
	var offset := 1.0 / float(resolution)
	
	return [
		Vector3(position.x, position.y, position.z),
		Vector3(position.x+1, position.y, position.z),
		Vector3(position.x+1, position.y, position.z+1),
		Vector3(position.x, position.y, position.z+1),
		Vector3(position.x, position.y+1, position.z),
		Vector3(position.x+1, position.y+1, position.z),
		Vector3(position.x+1, position.y+1, position.z+1),
		Vector3(position.x, position.y+1, position.z+1),
	]

func get_cube_values(noise: FastNoiseLite, cube_vertices: Array[Vector3], position: Vector3) -> Array[float]:
	return [
		1 - cube_vertices[0].y + 32 * noise.get_noise_3d(cube_vertices[0].x + position.x, cube_vertices[0].y + position.y, cube_vertices[0].z + position.z),
		1 - cube_vertices[1].y + 32 * noise.get_noise_3d(cube_vertices[1].x + position.x, cube_vertices[1].y + position.y, cube_vertices[1].z + position.z),
		1 - cube_vertices[2].y + 32 * noise.get_noise_3d(cube_vertices[2].x + position.x, cube_vertices[2].y + position.y, cube_vertices[2].z + position.z),
		1 - cube_vertices[3].y + 32 * noise.get_noise_3d(cube_vertices[3].x + position.x, cube_vertices[3].y + position.y, cube_vertices[3].z + position.z),
		1 - cube_vertices[4].y + 32 * noise.get_noise_3d(cube_vertices[4].x + position.x, cube_vertices[4].y + position.y, cube_vertices[4].z + position.z),
		1 - cube_vertices[5].y + 32 * noise.get_noise_3d(cube_vertices[5].x + position.x, cube_vertices[5].y + position.y, cube_vertices[5].z + position.z),
		1 - cube_vertices[6].y + 32 * noise.get_noise_3d(cube_vertices[6].x + position.x, cube_vertices[6].y + position.y, cube_vertices[6].z + position.z),
		1 - cube_vertices[7].y + 32 * noise.get_noise_3d(cube_vertices[7].x + position.x, cube_vertices[7].y + position.y, cube_vertices[7].z + position.z),
	]
	
func add_cubes_vertices(mesh: ImmediateMesh, cube_vertices: Array[Vector3]) -> void:
	mesh.surface_add_vertex(cube_vertices[0])
	mesh.surface_add_vertex(cube_vertices[1])
	
	mesh.surface_add_vertex(cube_vertices[1])
	mesh.surface_add_vertex(cube_vertices[2])
	
	mesh.surface_add_vertex(cube_vertices[2])
	mesh.surface_add_vertex(cube_vertices[3])
	
	mesh.surface_add_vertex(cube_vertices[0])
	mesh.surface_add_vertex(cube_vertices[3])
	
	mesh.surface_add_vertex(cube_vertices[0])
	mesh.surface_add_vertex(cube_vertices[4])
	
	mesh.surface_add_vertex(cube_vertices[2])
	mesh.surface_add_vertex(cube_vertices[6])
	
	mesh.surface_add_vertex(cube_vertices[5])
	mesh.surface_add_vertex(cube_vertices[6])
		
	mesh.surface_add_vertex(cube_vertices[5])
	mesh.surface_add_vertex(cube_vertices[4])
	
	mesh.surface_add_vertex(cube_vertices[5])
	mesh.surface_add_vertex(cube_vertices[1])
	
	mesh.surface_add_vertex(cube_vertices[6])
	mesh.surface_add_vertex(cube_vertices[7])
	
	mesh.surface_add_vertex(cube_vertices[4])
	mesh.surface_add_vertex(cube_vertices[7])
	
	mesh.surface_add_vertex(cube_vertices[3])
	mesh.surface_add_vertex(cube_vertices[7])

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
	var t: float = (cutoff - value1) / (value2 - value1)
	return Vector3(
		vertex1.x + t * (vertex2.x - vertex1.x),
		vertex1.y + t * (vertex2.y - vertex1.y),
		vertex1.z + t * (vertex2.z - vertex1.z),
	)
