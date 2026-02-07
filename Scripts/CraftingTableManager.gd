extends Interactable

@export var light: OmniLight3D
@export var audioPlayer: AudioStreamPlayer3D
@export var audioSwitchOn: AudioStreamMP3
@export var audioSwitchOff: AudioStreamMP3

func _ready():
	light.visible = false

func on_hover_enter() -> void:
	light.visible = true
	audioPlayer.stream = audioSwitchOn
	audioPlayer.play()

func on_hover_exit() -> void:
	light.visible = false
	audioPlayer.stream = audioSwitchOff
	audioPlayer.play()

func on_interact() -> void:
	print("Open Crafting Table UI")
