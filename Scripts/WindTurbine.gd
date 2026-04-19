@tool
extends Node3D

@export var animaton_player: AnimationPlayer
@export var working: bool = true:
	set(value):
		working = value
		if animaton_player:
			if working:
				animaton_player.play("Working")
			else:
				animaton_player.stop()

func _ready():
	if working:
		animaton_player.play("Working")
	else:
		animaton_player.stop()
