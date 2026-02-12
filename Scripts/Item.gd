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
	MISC
}

@export var item_type: ItemType = ItemType.MISC

func is_stackable() -> bool:
	return max_stack > 1
