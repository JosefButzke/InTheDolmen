extends Node
class_name Interactable

func on_hover_enter() -> void:
	print("[BASE] Show UI")

func on_hover_exit() -> void:
	print("[BASE] Remove UI")

func on_interact() -> void:
	print("[BASE] Collected")
