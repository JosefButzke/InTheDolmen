extends Camera3D

@export var mouse_sensitivity: float = 0.002
@export var max_look_up: float = 80.0
@export var max_look_down: float = -80.0
@export var cable_material: Material
@export var cable_radius: float = 0.01
@export var cable_sides: int = 8
@export var cable_surface_offset: float = 0.02
@export var cable_scroll_speed: float = 0.5

var pitch := 0.0

var cable_points: PackedVector3Array = []
var immediate_mesh: ImmediateMesh
var mesh_instance: MeshInstance3D


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pitch = rotation.x
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

	immediate_mesh = ImmediateMesh.new()
	mesh_instance = MeshInstance3D.new()
	mesh_instance.mesh = immediate_mesh
	mesh_instance.top_level = true # keep vertices in world space, independent of camera transform
	if cable_material:
		mesh_instance.material_override = cable_material
	get_tree().current_scene.add_child.call_deferred(mesh_instance)


func _process(delta: float) -> void:
	if Input.is_action_just_pressed("ESC"):
		if Input.mouse_mode == Input.MOUSE_MODE_VISIBLE:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)

	if cable_material is BaseMaterial3D:
		var mat := cable_material as BaseMaterial3D
		var offset := mat.uv1_offset
		offset.y = fmod(offset.y + delta * cable_scroll_speed, 1.0)
		mat.uv1_offset = offset

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		rotate_y(-event.relative.x * mouse_sensitivity)
		pitch -= event.relative.y * mouse_sensitivity
		pitch = clamp(pitch, deg_to_rad(max_look_down), deg_to_rad(max_look_up))
		rotation.x = pitch

	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		raycast_from_camera()


func raycast_from_camera(max_distance: float = 100.0) -> Variant:
	var from := global_position
	var to := from + -global_transform.basis.z * max_distance

	var space_state := get_world_3d().direct_space_state
	var query := PhysicsRayQueryParameters3D.create(from, to)
	var result := space_state.intersect_ray(query)
	
	if result:
		var outlet := _find_outlet(result.collider)
		if cable_points.is_empty() and not outlet:
			return null # only allow starting a new cable from an outlet
		if outlet:
			print("outlet")
			add_cable_point(outlet.global_position)
		else:
			add_cable_point(result.position, result.normal)
		return result.position
	return null


func _find_outlet(node: Node) -> Node3D:
	var ancestor := node
	while ancestor:
		if ancestor.scene_file_path == "res://scenes/constructions/outlet.tscn":
			return ancestor
		ancestor = ancestor.get_parent()
	return null


func add_cable_point(point: Vector3, surface_normal: Vector3 = Vector3.ZERO) -> void:
	cable_points.append(point + surface_normal * cable_surface_offset)
	_redraw_cable()


func _redraw_cable() -> void:
	immediate_mesh.clear_surfaces()
	if cable_points.size() < 2:
		return

	immediate_mesh.surface_begin(Mesh.PRIMITIVE_TRIANGLES)
	_add_tube(cable_points)
	for i in range(1, cable_points.size() - 1):
		_add_joint_sphere(cable_points[i])
	immediate_mesh.surface_end()


func _add_tube(path: PackedVector3Array) -> void:
	var rings: Array[PackedVector3Array] = []
	var v_coords: Array[float] = []
	var length_so_far := 0.0

	for i in range(path.size()):
		var tangent: Vector3
		if i == 0:
			tangent = (path[i + 1] - path[i]).normalized()
		elif i == path.size() - 1:
			tangent = (path[i] - path[i - 1]).normalized()
		else:
			tangent = (path[i + 1] - path[i - 1]).normalized()

		if i > 0:
			length_so_far += path[i].distance_to(path[i - 1])
		v_coords.append(length_so_far)

		rings.append(_make_ring(path[i], tangent))

	for i in range(rings.size() - 1):
		var ring_a := rings[i]
		var ring_b := rings[i + 1]
		var v_a := v_coords[i]
		var v_b := v_coords[i + 1]

		for s in range(cable_sides):
			var s_next := (s + 1) % cable_sides
			var u_a := float(s) / float(cable_sides)
			var u_b := float(s_next) / float(cable_sides)

			var a := ring_a[s]
			var b := ring_a[s_next]
			var c := ring_b[s]
			var d := ring_b[s_next]

			_add_triangle(a, c, b, Vector2(u_a, v_a), Vector2(u_a, v_b), Vector2(u_b, v_a))
			_add_triangle(b, c, d, Vector2(u_b, v_a), Vector2(u_a, v_b), Vector2(u_b, v_b))


# Covers the miter seam at a bend with a small sphere sized to the tube's
# radius, so corners read as a smooth rounded joint instead of a sharp miter.
func _add_joint_sphere(center: Vector3) -> void:
	@warning_ignore("integer_division")
	var lat_steps: int = maxi(4, cable_sides / 2)

	for lat in range(lat_steps):
		var theta0 := PI * float(lat) / float(lat_steps)
		var theta1 := PI * float(lat + 1) / float(lat_steps)

		for lon in range(cable_sides):
			var phi0 := TAU * float(lon) / float(cable_sides)
			var phi1 := TAU * float(lon + 1) / float(cable_sides)

			var p00 := center + _sphere_point(theta0, phi0)
			var p01 := center + _sphere_point(theta0, phi1)
			var p10 := center + _sphere_point(theta1, phi0)
			var p11 := center + _sphere_point(theta1, phi1)

			var uv00 := Vector2(phi0 / TAU, theta0 / PI)
			var uv01 := Vector2(phi1 / TAU, theta0 / PI)
			var uv10 := Vector2(phi0 / TAU, theta1 / PI)
			var uv11 := Vector2(phi1 / TAU, theta1 / PI)

			_add_triangle(p00, p10, p01, uv00, uv10, uv01)
			_add_triangle(p01, p10, p11, uv01, uv10, uv11)


func _sphere_point(theta: float, phi: float) -> Vector3:
	return Vector3(sin(theta) * cos(phi), cos(theta), sin(theta) * sin(phi)) * cable_radius


func _make_ring(center: Vector3, tangent: Vector3) -> PackedVector3Array:
	var up := Vector3.UP
	if abs(tangent.dot(up)) > 0.99:
		up = Vector3.RIGHT

	var right := tangent.cross(up).normalized()
	var normal := right.cross(tangent).normalized()

	var ring: PackedVector3Array = []
	for s in range(cable_sides):
		var angle := TAU * float(s) / float(cable_sides)
		var offset := right * cos(angle) * cable_radius + normal * sin(angle) * cable_radius
		ring.append(center + offset)

	return ring


func _add_triangle(a: Vector3, b: Vector3, c: Vector3, uv_a: Vector2, uv_b: Vector2, uv_c: Vector2) -> void:
	var face_normal := (b - a).cross(c - a).normalized()
	immediate_mesh.surface_set_normal(face_normal)
	immediate_mesh.surface_set_uv(uv_a)
	immediate_mesh.surface_add_vertex(a)
	immediate_mesh.surface_set_normal(face_normal)
	immediate_mesh.surface_set_uv(uv_b)
	immediate_mesh.surface_add_vertex(b)
	immediate_mesh.surface_set_normal(face_normal)
	immediate_mesh.surface_set_uv(uv_c)
	immediate_mesh.surface_add_vertex(c)
