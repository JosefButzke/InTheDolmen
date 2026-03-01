extends Control

@export var items: Array[Item] = []
@export var slotsUI: Array[TextureButton] = []
@export var slotsNumber = 40

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var rows = get_children()
	
	for i in range(rows.size()):
		for child in rows[i].get_children():
			if child is TextureButton:
				slotsUI.append(child)
		
	slotsNumber = slotsUI.size()
	items.resize(slotsNumber)
	updateInventoryUI()

func updateInventoryUI():
	for i in range(items.size()):
		if items[i]:
			var slotLabel: Label = slotsUI[i].get_node("Label")
			var slotSprite: TextureRect = slotsUI[i].get_node("TextureRect")
		
			slotLabel.text = str(i)
			slotLabel.visible = true
			slotSprite.texture = items[i].icon
			slotSprite.visible = true
	print("Inventory UI updated")
