extends Node3D

var speed: float = 5.0

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	var position_spike = position
	position_spike.y = 0.0;
	var position_center = Vector3.ZERO
	position_center.y = 0.0
	
	var direction = (position_spike - position_center).normalized()
	position += direction * speed * delta
