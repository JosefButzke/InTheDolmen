extends RefCounted
class_name HotbarSlot

var item: Item
var slot: TextureButton

func _init(p_slot: TextureButton) -> void:
	slot = p_slot
