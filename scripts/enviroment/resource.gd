extends Interactable

@export var item: Item

func on_hover_enter() -> void:
	print("Show UI")
	Events.ui_interact_icon_show.emit(item.item_type, item.name)

func on_hover_exit() -> void:
	Events.ui_interact_icon_hide.emit()

func on_interact() -> void:
	print("Call collect resource")
	Events.ui_interact_icon_hide.emit()
