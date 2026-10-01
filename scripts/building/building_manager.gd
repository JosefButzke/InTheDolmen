extends Node3D

var building_parts: Array[PackedScene] = [
	preload("res://scenes/base_system/frame_v.tscn"),
	preload("res://scenes/base_system/frame_h.tscn"),
	preload("res://scenes/base_system/wall.tscn"),
	preload("res://scenes/base_system/floor.tscn"),
	preload("res://scenes/base_system/ceiling.tscn"),
	preload("res://scenes/constructions/lamp.tscn"),
]

var selected_part_index: int = 0
var selected_snap_index: int = 0

var ghost: Node3D = null
var ghost_origin_position = null
var ghost_rotation: float = 0.0 # radians, around the ghost's local Y

var enabled: bool = false:
	set(value):
		enabled = value
		Events.building_active = value
		Events.building_mode_changed.emit(value) # placement points show/hide their markers

func _ready():
	enabled = true;
	Events.building_place_ghost.connect(place_ghost)
	Events.building_remove_ghost.connect(remove_ghost)
	Events.building_apply_ghost.connect(apply_ghost)
	Events.building_part_selected.connect(change_part_selected)

func _unhandled_input(event: InputEvent) -> void:
	if not enabled:
		return
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_RIGHT and event.pressed:
		Events.building_toggle_menu.emit()
		get_viewport().set_input_as_handled()
	elif event is InputEventKey and event.pressed and not event.echo:
		match event.keycode:
			KEY_R:
				update_ghost_rotation()
				get_viewport().set_input_as_handled()
			KEY_Q:
				update_ghost_snap_position()
				get_viewport().set_input_as_handled()

func change_part_selected(new_index: int) -> void:
	selected_snap_index = 0
	selected_part_index = new_index
	
	if (ghost != null):
		update_ghost_visual()

func place_ghost(pos: Vector3) -> void:
	if not enabled:
		return
	remove_ghost()
	if (ghost_origin_position == null):
		ghost_origin_position = pos;
	var building_part: PackedScene = building_parts[selected_part_index]
	ghost = building_part.instantiate()
	_disable_collision(ghost) # so the ghost's own placement points don't catch the raycast
	get_tree().current_scene.add_child(ghost)
	ghost.rotate_y(ghost_rotation)
	var snap_positions = get_snap_points()
	# rotate the snap offset too, so the ghost pivots around the selected snap point
	ghost.global_position = pos - ghost.global_basis * snap_positions[selected_snap_index].position

func remove_ghost() -> void:
	if ghost:
		ghost.queue_free()
		ghost = null
		ghost_origin_position = null

func apply_ghost() -> void:
	if not enabled or ghost == null:
		return
	var building_part: PackedScene = building_parts[selected_part_index]
	var part: Node3D = building_part.instantiate()
	part.add_to_group(SaveGame.PERSIST_GROUP)
	get_tree().current_scene.add_child(part)
	part.global_transform = ghost.global_transform
	_play_place_animation(part)

	remove_ghost()

# quick squash-and-stretch bounce that settles back to the original scale
func _play_place_animation(part: Node3D) -> void:
	var base_scale := part.scale
	var tween := part.create_tween()
	tween.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	for factor in [1.15, 0.92, 1.05, 0.98, 1.0]:
		tween.tween_property(part, "scale", base_scale * factor, 0.07)

func update_ghost_visual() -> void:
	var pos = ghost_origin_position
	place_ghost(pos)

func update_ghost_snap_position() -> void:
	if (ghost == null):
		return
	var snap_positions = get_snap_points()
	if (snap_positions.size() == selected_snap_index + 1):
		selected_snap_index = 0
	else:
		selected_snap_index = selected_snap_index + 1
	place_ghost(ghost_origin_position)

func update_ghost_rotation() -> void:
	if (ghost == null):
		return
	ghost_rotation = wrapf(ghost_rotation + PI / 2, 0, TAU)
	place_ghost(ghost_origin_position)

func get_snap_points() -> Array[CollisionObject3D]:
	var placement = ghost.get_node("Placement")
	return placement.get_children() as Array[CollisionObject3D]

func _disable_collision(node: Node) -> void:
	if node is CollisionObject3D:
		node.collision_layer = 0
		node.collision_mask = 0
	for child in node.get_children():
		_disable_collision(child)

func on_equip() -> void:
	enabled = true

func on_unequip() -> void:
	enabled = false
	remove_ghost()
