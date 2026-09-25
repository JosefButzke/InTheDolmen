extends Node3D

var building_parts: Array[PackedScene] = [
	preload("res://scenes/base_system/frame.tscn"),
	preload("res://scenes/base_system/wall.tscn"),
	preload("res://scenes/base_system/floor.tscn"),
	preload("res://scenes/base_system/stairs.tscn"),
]
var selected_part_index: int = 0

var ghost: Node3D = null

func change_part_selected(new_index: int) -> void:
	selected_part_index = new_index
	
	if(ghost != null):
		update_ghost_visual()
	

func place_ghost(pos: Vector3) -> void:
	remove_ghost()
	var building_part: PackedScene = building_parts[selected_part_index]
	ghost = building_part.instantiate()
	_disable_collision(ghost) # so the ghost's own placement points don't catch the raycast
	get_tree().current_scene.add_child(ghost)
	ghost.global_position = pos

func remove_ghost() -> void:
	if ghost:
		ghost.queue_free()
		ghost = null

func apply_ghost() -> void:
	if ghost == null:
		return
	var building_part: PackedScene = building_parts[selected_part_index]
	var part: Node3D = building_part.instantiate()
	get_tree().current_scene.add_child(part)
	part.global_transform = ghost.global_transform
	remove_ghost()
	
func update_ghost_visual() -> void:
	var pos = ghost.global_position
	remove_ghost()
	place_ghost(pos)	

func _disable_collision(node: Node) -> void:
	if node is CollisionObject3D:
		node.collision_layer = 0
		node.collision_mask = 0
	for child in node.get_children():
		_disable_collision(child)
