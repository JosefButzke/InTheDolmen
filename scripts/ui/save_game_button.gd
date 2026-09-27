extends Button

func _on_pressed() -> void:
	SaveGame.save_game()
	get_tree().quit()
