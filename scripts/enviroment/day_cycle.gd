extends DirectionalLight3D

@export var day_duration: float # seconds
@export var night_duration: float # seconds
@export var env: WorldEnvironment

var current_time: float

func _ready() -> void:
	rotation.x = deg_to_rad(-90);
	current_time = 0.0;

func _process(delta: float) -> void:
	current_time += delta;

	var cycle_duration = day_duration + night_duration;
	current_time = fmod(current_time, cycle_duration);

	var brightness: float;
	if current_time < day_duration:
		var t = current_time / day_duration # 0 → 1 during day
		brightness = sin(t * PI);
	else:
		brightness = 0.0;

	env.environment.background_energy_multiplier = brightness;
	light_energy = brightness;
