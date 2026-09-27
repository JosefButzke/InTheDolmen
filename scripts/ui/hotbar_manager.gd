extends Control
class_name Hotbar

@export var slot_tex_default: Texture2D
@export var slot_tex_selected: Texture2D

var slots: Array[HotbarSlot] = []
var _selected_index: int = -1

func _ready() -> void:
	for panel in $HBoxContainer.get_children():
		var slot := panel.get_node_or_null("Slot") as TextureButton
		if slot:
			slots.append(HotbarSlot.new(slot))

func select_slot(index: int) -> void:
	if index < 0 or index >= slots.size():
		return
		
	if(_selected_index == index):
		unselect_slot(index)
		return

	_selected_index = index
	for i in slots.size():
		if i == _selected_index:
			slots[i].slot.texture_normal = slot_tex_selected
			Events.equip_item.emit(slots[index].item)
		else:
			slots[i].slot.texture_normal = slot_tex_default
			
func unselect_slot(index: int) -> void:
	if index < 0 or index >= slots.size():
		return

	slots[index].slot.texture_normal = slot_tex_default
	Events.unequip_item.emit(slots[index].item)
	_selected_index = -1

func add_item(slot_number: int, item: Item) -> void:
	var index = slot_number - 1;
	if index < 0 or index >= slots.size():
		return
	slots[index].item = item
	var slot_item_tex := slots[index].slot.get_node_or_null("ContentItemTex") as TextureRect
	if slot_item_tex:
		slot_item_tex.texture = item.icon
