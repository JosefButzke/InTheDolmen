extends Resource
class_name Item

@export var name: String = ""
@export var description: String = ""

@export var icon: Texture2D
@export var max_stack: int = 99
@export var weight: float = 0.0

enum ItemType {
	TOOL,
	MACHINE,
	MODULE,
	VEHICLE,
	RESOURCE,
	MATERIAL,
	Building,
	MISC
}

@export var item_type: ItemType = ItemType.MISC

@export var equip_scene: PackedScene

func is_stackable() -> bool:
	return max_stack > 1
