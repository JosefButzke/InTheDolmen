extends Node3D

@export var beacon_deploy_point: Node3D
@export var beacon: PackedScene
@export var throw_speed: float = 20.0
@export var flashlight: SpotLight3D

func _ready() -> void:
	pass

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		var instance = beacon.instantiate()
		var camera = get_viewport().get_camera_3d()
		var direction = -camera.global_transform.basis.z
		instance.global_transform = Transform3D(Basis.looking_at(direction, camera.global_transform.basis.y), beacon_deploy_point.global_position)
		instance.setup(direction)
		get_tree().current_scene.add_child(instance)
	if event is InputEventKey:
		if event.keycode == KEY_F and event.pressed and not event.echo:
			flashlight.visible = !flashlight.visible
