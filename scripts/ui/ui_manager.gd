extends Control
class_name UIManager

@export var inventory: Control
@export var hotbar: Hotbar
@export var menu: Control

@export var icon_container: VBoxContainer
@export var icon_tex: TextureRect
@export var icon_label: Label

@export var item_test_hotbar_1: Item #temp
@export var item_test_hotbar_2: Item #temp

func _ready() -> void:
	inventory.visible = false
	menu.visible = false
	icon_container.visible = false
	
	Events.ui_interact_icon_show.connect(_interact_icon_show)
	Events.ui_interact_icon_hide.connect(_interact_icon_hide)
	
	hotbar.add_item(1, item_test_hotbar_1)
	hotbar.add_item(2, item_test_hotbar_2)

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("Inventory"):
		toggle_inventory()
		get_viewport().set_input_as_handled()
		return

	if event is InputEventKey and event.pressed and not event.echo:
		if event.keycode >= KEY_1 and event.keycode <= KEY_7:
			select_hotbar_slot(event.keycode - KEY_1)
			get_viewport().set_input_as_handled()
	
	if event.is_action_pressed("ESC"):
		toggle_menu()
		get_viewport().set_input_as_handled()

func toggle_inventory() -> void:
	inventory.visible = not inventory.visible
	_update_mouse_mode()

func toggle_menu() -> void:
	menu.visible = not menu.visible
	_update_mouse_mode()

# free the cursor while any panel is open, capture it again once all are closed
func _update_mouse_mode() -> void:
	if inventory.visible or menu.visible:
		Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
	else:
		Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func select_hotbar_slot(index: int) -> void:
	hotbar.select_slot(index)

func _interact_icon_show(icon_name: Item.ItemType, label: String) -> void:
	icon_tex.texture = preload("res://images/ore.png")
	icon_container.visible = true;
	icon_label.text = label
	
func _interact_icon_hide() -> void:
	icon_container.visible = false
	icon_label.text = ""
