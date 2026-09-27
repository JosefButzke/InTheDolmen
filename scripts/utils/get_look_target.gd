extends Node3D

class_name Utils

static func get_look_target(camera: Camera3D, ray_length: float = 100.0, collision_mask: int = 1, exclude: Array[RID] = [], collide_with_areas: bool = false) -> Dictionary:
	var space_state = camera.get_world_3d().direct_space_state

	var origin = camera.global_position
	var forward = - camera.global_transform.basis.z
	var end = origin + forward * ray_length

	var query = PhysicsRayQueryParameters3D.create(origin, end, collision_mask, exclude)
	query.collide_with_areas = collide_with_areas

	var result = space_state.intersect_ray(query)
	return result
