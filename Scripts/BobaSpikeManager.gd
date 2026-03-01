extends Node3D

@export var audioPlayer: AudioStreamPlayer3D
@export var alertRadius = 10.0
@export var shootRadius = 5.0
@export var spikeSpeed = 15.0

var body: MeshInstance3D
var spikes: Array[MeshInstance3D]
var spike_initial_positions: Array[Vector3]
var spike_script_added: bool = false
var player: Node3D

func _ready() -> void:
	player = get_tree().get_first_node_in_group("player")
	if(player == null):
		print("Player ref missing to: ", self.name)
	for child in get_children():
		if child.name.begins_with("Spike") and child is MeshInstance3D:
			spikes.append(child)
			spike_initial_positions.append(child.position)
		if child.name.begins_with("Body") and child is MeshInstance3D:
			body = child

func _process(delta: float) -> void:
	if player == null:
		print("Player missing")
		return
	if body == null or spikes == null:
		print("Body/Spikes missing")
		return

	var distance = global_position.distance_to(player.global_position)

	# Scale body between 1.0 (at 5m) and 1.5 (at 0m)
	if distance < alertRadius:
		var t = 1.0 - (distance / alertRadius)
		var sXZ = lerp(1.0, 1.5, t)
		var sY = lerp(1.0, 1.3, t)
		body.scale = Vector3.ONE * Vector3(sXZ, sY, sXZ)
	else:
		body.scale = Vector3.ONE

	# Attach Spike.gd to spikes node when player is within 2m
	if distance < shootRadius and not spike_script_added:
		spike_script_added = true
		audioPlayer.play()
		_reset_spikes_after_delay()
	
	if(spike_script_added):
		shoot_spikes(delta)
		
func shoot_spikes(delta: float) -> void:
	var position_center = Vector3.ZERO
	position_center.y = 0.0
	print(spikes.size())
	for spike in spikes:
		var position_spike = spike.position
		position_spike.y = 0.0;
		var direction = (position_spike - position_center).normalized()
		spike.position += direction * spikeSpeed * delta

func _reset_spikes_after_delay() -> void:
	await get_tree().create_timer(1.0).timeout
	for i in spikes.size():
		spikes[i].position = spike_initial_positions[i]
	spike_script_added = false
