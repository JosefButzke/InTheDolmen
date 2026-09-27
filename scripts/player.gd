extends Node3D

enum MovementMode {NORMAL, SPHERE}
@export var movement_mode: MovementMode = MovementMode.SPHERE

@export var speed: float = 6.0
@export var jump_velocity: float = 5.0
@export var mouse_sensitivity: float = 0.002

@export var characterBody: CharacterBody3D
@export var camera_pivot: Node3D
@export var camera: Camera3D
@export_range(0, 16) var RAY_LENGTH: int = 6:
	set(value):
		RAY_LENGTH = value
@export var hand_pivot: Node3D
@export var animationPlayer: AnimationPlayer

@export var gravity_strength: float = 9.8
@export var align_speed: float = 15.0

var current_interactable_object: Interactable = null

var max_look_up := 80.0
var max_look_down := -80.0
var pitch := 0.0

var vertical_velocity: float = 0.0

var _equipped: Node = null

func _ready():
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	Events.equip_item.connect(equip_item)
	Events.unequip_item.connect(unequip_item)

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		rotate_y(-event.relative.x * mouse_sensitivity)
		pitch -= event.relative.y * mouse_sensitivity
		pitch = clamp(pitch, deg_to_rad(max_look_down), deg_to_rad(max_look_up))
		camera_pivot.rotation.x = pitch
		return

	elif event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		if Input.mouse_mode == Input.MOUSE_MODE_CAPTURED and current_interactable_object:
			current_interactable_object.on_interact()

# Smoothly rotates self so its Y-axis aligns with planet_up.
# Takes delta so it never snaps — max rotation is align_speed rad/s.
func _align_to_planet(planet_up: Vector3, delta: float) -> void:
	var current_up := global_transform.basis.y
	var rotation_axis := current_up.cross(planet_up)
	if rotation_axis.length_squared() < 0.00001:
		return
	var angle := current_up.angle_to(planet_up)
	var step := minf(angle, align_speed * delta)
	quaternion = (Quaternion(rotation_axis.normalized(), step) * quaternion).normalized()

func _physics_process(delta: float) -> void:
	var planet_up: Vector3
	if movement_mode == MovementMode.SPHERE:
		planet_up = global_position.normalized()
		if planet_up == Vector3.ZERO:
			planet_up = Vector3.UP
		_align_to_planet(planet_up, delta)
	else:
		planet_up = Vector3.UP

	# Tell CharacterBody3D which direction is "up" for floor detection and sliding.
	characterBody.up_direction = planet_up

	if characterBody.is_on_floor():
		if vertical_velocity < 0.0:
			vertical_velocity = 0.0
	else:
		vertical_velocity -= gravity_strength * delta

	if Input.is_action_just_pressed("jump") and characterBody.is_on_floor():
		vertical_velocity = jump_velocity

	var input_dir := Vector2(
		Input.get_action_strength("move_right") - Input.get_action_strength("move_left"),
		Input.get_action_strength("move_backward") - Input.get_action_strength("move_forward")
	)

	if Input.get_action_strength("move_forward") > 0.0:
		animationPlayer.play("Walk")
	else:
		animationPlayer.stop()

	var local_move := Vector3(input_dir.x, 0.0, input_dir.y)
	var world_move := global_transform.basis * local_move
	world_move = (world_move - planet_up * world_move.dot(planet_up)).normalized()

	var current_horizontal := characterBody.velocity - planet_up * characterBody.velocity.dot(planet_up)

	if local_move.length() > 0.0:
		current_horizontal = world_move * speed
	else:
		current_horizontal = current_horizontal.move_toward(Vector3.ZERO, speed * delta * 10.0)

	characterBody.velocity = current_horizontal + planet_up * vertical_velocity
	characterBody.move_and_slide()

	var result = Utils.get_look_target(camera, RAY_LENGTH, Constants.interactableLayer, [characterBody.get_rid()], true)
	var new_interactable_object: Interactable = null

	if not result.is_empty():
		var collider = result["collider"]

		if collider is Interactable:
			new_interactable_object = collider
		elif collider is CollisionObject3D:
			# the ray returns the body, so find which CollisionShape3D was hit
			var shape_owner = collider.shape_owner_get_owner(collider.shape_find_owner(result["shape"]))
			if shape_owner is Interactable:
				new_interactable_object = shape_owner

	if new_interactable_object != current_interactable_object:
		if current_interactable_object:
			current_interactable_object.on_hover_exit()
		if new_interactable_object:
			new_interactable_object.on_hover_enter()
		current_interactable_object = new_interactable_object


func equip_item(item: Item) -> void:
	if _equipped:
		if _equipped.has_method("on_unequip"):
			_equipped.on_unequip()
		_equipped.queue_free()
		_equipped = null

	if item and item.equip_scene:
		_equipped = item.equip_scene.instantiate()
		hand_pivot.add_child(_equipped)
		if _equipped.has_method("on_equip"):
			_equipped.on_equip()

func unequip_item(_item: Item) -> void:
	if _equipped:
		if _equipped.has_method("on_unequip"):
			_equipped.on_unequip()
		_equipped.queue_free()
		_equipped = null
