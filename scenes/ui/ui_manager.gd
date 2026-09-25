extends Control
class_name UIManager

static var instance: UIManager

@export var inventory: Control
@export var hotbar: Hotbar

func _ready() -> void:
	instance = self
	inventory.visible = false

func select_hotbar_slot(index: int) -> void:
	hotbar.select_slot(index)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:			
	if Input.is_action_just_pressed("Inventory"):
		if inventory.visible:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
			inventory.visible = false
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
			inventory.visible = true
