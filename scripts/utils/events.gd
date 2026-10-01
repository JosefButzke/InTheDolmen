extends Node

signal equip_item(item: Item)
signal unequip_item(item: Item)

signal building_place_ghost(position: Vector3)
signal building_remove_ghost()
signal building_apply_ghost()
signal building_toggle_menu()
signal building_part_selected(index: int)
signal building_mode_changed(active: bool)

var building_active: bool = false # last value sent through building_mode_changed

signal ui_interact_icon_show(icon_name: Item.ItemType, label: String)
signal ui_interact_icon_hide()
