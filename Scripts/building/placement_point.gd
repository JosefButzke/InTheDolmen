extends Interactable

var filled: bool = false
	
func on_hover_enter() -> void:
	if !filled:
		filled = true;
		BuildingManager.place_ghost(self.global_position)
	
	print("Ghost Placed")

func on_hover_exit() -> void:
	filled = false;
	BuildingManager.remove_ghost()
	print("Ghost Removed")

func on_interact() -> void:
	BuildingManager.apply_ghost()
	print("Part Placed")
