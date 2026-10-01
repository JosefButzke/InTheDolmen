extends Interactable

static var _marker_material: StandardMaterial3D

var filled: bool = false
var _marker: MeshInstance3D

func _ready() -> void:
	_marker = MeshInstance3D.new()
	var sphere := SphereMesh.new()
	var shape = get("shape")
	var radius := 0.15
	if shape is SphereShape3D:
		radius = shape.radius
	elif shape is BoxShape3D:
		var s: Vector3 = shape.size
		radius = minf(s.x, minf(s.y, s.z)) * 0.5
	sphere.radius = radius
	sphere.height = radius * 2.0
	sphere.material = _get_marker_material()
	_marker.mesh = sphere
	_marker.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	add_child(_marker)

	Events.building_mode_changed.connect(_on_building_mode_changed)
	_on_building_mode_changed(Events.building_active)

func on_hover_enter() -> void:
	if !filled:
		filled = true;
		Events.building_place_ghost.emit(self.global_position)

func on_hover_exit() -> void:
	filled = false;
	Events.building_remove_ghost.emit()

func on_interact() -> void:
	filled = true
	Events.building_apply_ghost.emit()

func _on_building_mode_changed(active: bool) -> void:
	_marker.visible = active and not _is_ghost()

# ghost parts get their collision cleared by the building manager
func _is_ghost() -> bool:
	var body := get_parent() as CollisionObject3D
	return body != null and body.collision_layer == 0

static func _get_marker_material() -> StandardMaterial3D:
	if _marker_material == null:
		_marker_material = StandardMaterial3D.new()
		_marker_material.albedo_color = Color(1.0, 0.85, 0.0, 0.5)
		_marker_material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
		_marker_material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	return _marker_material
