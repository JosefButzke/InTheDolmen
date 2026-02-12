extends Node3D

@export var speed: float = 5.0
@export var jump_velocity: float = 10
@export var mouse_sensitivity: float = 0.002

@export var characterBody: CharacterBody3D
@export var camera_pivot: Node3D
@export var camera: Camera3D
@export_range(0, 16) var RAY_LENGTH: int = 8:
	set(value):
		RAY_LENGTH = value
		
var current_interactable_object: Interactable = null

var gravity: float = ProjectSettings.get_setting("physics/3d/default_gravity")

var max_look_up := 80.0
var max_look_down := -80.0
var pitch := 0.0

func _ready():
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	
func _unhandled_input(event):
	if event is InputEventMouseMotion && Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		# Yaw (character)
		rotate_y(-event.relative.x * mouse_sensitivity)

		# Pitch (camera)
		pitch -= event.relative.y * mouse_sensitivity
		pitch = clamp(pitch, deg_to_rad(max_look_down), deg_to_rad(max_look_up))
		camera_pivot.rotation.x = pitch
		
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	if Input.is_action_just_pressed("ESC"):
		if(Input.mouse_mode == Input.MOUSE_MODE_VISIBLE):
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)

func _physics_process(delta: float) -> void:
	# Apply gravity
	if not characterBody.is_on_floor():
		characterBody.velocity.y -= gravity * delta

	# Jump
	if Input.is_action_just_pressed("jump") and characterBody.is_on_floor():
		characterBody.velocity.y = jump_velocity

	# Movement input
	var input_dir := Vector2(
		Input.get_action_strength("move_right") - Input.get_action_strength("move_left"),
		Input.get_action_strength("move_backward") - Input.get_action_strength("move_forward")
	)

	var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()

	if direction: 
		characterBody.velocity.x = direction.x * speed
		characterBody.velocity.z = direction.z * speed
	else:
		characterBody.velocity.x = move_toward(characterBody.velocity.x, 0, speed)
		characterBody.velocity.z = move_toward(characterBody.velocity.z, 0, speed)
		
	characterBody.move_and_slide()
	
	var space_state = get_world_3d().direct_space_state
	var mousepos = get_viewport().get_mouse_position()
	
	var origin = camera.project_ray_origin(mousepos)
	var end = origin + camera.project_ray_normal(mousepos) * RAY_LENGTH
	var query = PhysicsRayQueryParameters3D.create(origin, end, Constants.interactableLayer)
	query.exclude = [self]
	query.collide_with_areas = true

	var result = space_state.intersect_ray(query)
	
	var new_interactable_object: Interactable = null
	
	if not result.is_empty():
		var collider = result["collider"]
		var parent = collider.get_parent()
		
		if parent is Interactable:
			new_interactable_object = parent
			
	if new_interactable_object != current_interactable_object:
		if current_interactable_object:
			current_interactable_object.on_hover_exit()
		if new_interactable_object:
			new_interactable_object.on_hover_enter()
		current_interactable_object = new_interactable_object
