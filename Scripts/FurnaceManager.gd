extends Interactable

@export var fire: MeshInstance3D
var working: bool = false

@export var audioPlayer: AudioStreamPlayer3D
@export var audioFireWorking: AudioStreamMP3
@export var audioFireStop: AudioStreamWAV

func _ready():
	fire.position = Vector3.UP * 1.03
	
func on_hover_enter() -> void:
	fire.position = Vector3.UP * 1.03 * 1.05
	working = true
	audioPlayer.stream = audioFireWorking
	audioPlayer.play()

func on_hover_exit() -> void:
	fire.position = Vector3.UP * 1.03
	fire.rotation = Vector3.ZERO
	working = false
	audioPlayer.stream = audioFireStop
	audioPlayer.play()

func on_interact() -> void:
	print("Open Crafting Table UI")

func _process(delta: float) -> void:
	if working:
		fire.rotate(Vector3.UP, 1*delta)
		
