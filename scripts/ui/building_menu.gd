extends Control

@onready var container: HBoxContainer = $HBoxContainer

var building_items: Array[Item] = [
	preload("res://items/base_system/parts/frame.tres"),
	preload("res://items/base_system/parts/wall.tres"),
	preload("res://items/base_system/parts/floor.tres"),
	preload("res://items/base_system/parts/stairs.tres"),
	preload("res://items/base_system/parts/lamp.tres"),
]


func _ready() -> void:
	visible = false
	Events.building_toggle_menu.connect(toggle_building_menu)

	var panels := container.get_children()
	for index in panels.size():
		var panel := panels[index]
		var slot := panel.get_node("Slot") as TextureButton
		var label := panel.get_node("Label") as Label
		var item: Item = building_items[index] if index < building_items.size() else null

		label.text = item.name if item else ""
		slot.get_node("ContentItemTex").texture = item.icon if item else null
		slot.disabled = item == null
		slot.pressed.connect(_on_slot_pressed.bind(index))

func toggle_building_menu() -> void:
	visible = not visible
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE if visible else Input.MOUSE_MODE_CAPTURED

func _on_slot_pressed(index: int) -> void:
	Events.building_part_selected.emit(index)
	toggle_building_menu()
