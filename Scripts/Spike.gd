extends Node3D

var speed: float = 15.0

func _ready():
	print("Spike init")

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	var position_spike = position
	position_spike.y = 0.0;
	var position_center = Vector3.ZERO
	position_center.y = 0.0
	
	var direction = (position_spike - position_center).normalized()
	position += direction * speed * delta
