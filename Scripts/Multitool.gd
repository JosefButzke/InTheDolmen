extends Node3D

@export var beacon: Node3D
@export var throw_speed: float = 20.0

var _thrown_beacon: Node3D = null
var _beacon_velocity: Vector3 = Vector3.ZERO
var _is_throwing: bool = false


func _ready() -> void:
	pass


func _process(delta: float) -> void:
	if _is_throwing and _thrown_beacon:
		_update_throw(delta)


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		_throw_beacon()


func _throw_beacon() -> void:
	if not beacon:
		return
	if _thrown_beacon:
		_thrown_beacon.queue_free()

	var camera := get_viewport().get_camera_3d()
	if not camera:
		return

	_thrown_beacon = beacon.duplicate()
	get_tree().root.add_child(_thrown_beacon)
	_thrown_beacon.global_position = global_position
	_beacon_velocity = - camera.global_basis.z * throw_speed
	_is_throwing = true


func _update_throw(delta: float) -> void:
	var from := _thrown_beacon.global_position
	var to := from + _beacon_velocity * delta

	var query := PhysicsRayQueryParameters3D.create(from, to)
	var result := get_world_3d().direct_space_state.intersect_ray(query)

	if result:
		_thrown_beacon.global_position = result.position + result.normal * 0.05
		_thrown_beacon.reparent(result.collider)
		_thrown_beacon = null
		_is_throwing = false
	else:
		_thrown_beacon.global_position = to
		_beacon_velocity.y -= 9.8 * delta
