extends Control

@export var inventory: Control

func _ready() -> void:
	inventory.visible = false
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:			
	if Input.is_action_just_pressed("Inventory"):
		if inventory.visible:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
			inventory.visible = false
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
			inventory.visible = true
