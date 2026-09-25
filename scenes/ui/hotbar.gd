extends Control
class_name Hotbar

@export var slots: Array[TextureButton] = []

var selected_index: int = -1
var _normal_textures: Array[Texture2D] = []

func _ready() -> void:
	if slots.is_empty():
		for panel in $HBoxContainer.get_children():
			var slot := panel.get_node_or_null("Slot")
			if slot is TextureButton:
				slots.append(slot)
	for slot in slots:
		_normal_textures.append(slot.texture_normal)
	select_slot(0)

func select_slot(index: int) -> void:
	if index < 0 or index >= slots.size():
		return
	if selected_index >= 0 and selected_index < slots.size():
		slots[selected_index].texture_normal = _normal_textures[selected_index]
	selected_index = index
	slots[selected_index].texture_normal = slots[selected_index].texture_hover
