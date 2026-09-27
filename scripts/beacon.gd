extends Node3D

@export var speed: float = 10.0
@export var direction: Vector3 = Vector3.FORWARD

func setup(_direction: Vector3):
	direction = _direction

func _physics_process(delta):
	var space_state = get_world_3d().direct_space_state
	var from = position
	var to = position + direction * speed * delta
	var query = PhysicsRayQueryParameters3D.create(from, to)
	var result = space_state.intersect_ray(query)
	if result and result.collider:
		position = to
		set_physics_process(false)
		return
	position = to
	
