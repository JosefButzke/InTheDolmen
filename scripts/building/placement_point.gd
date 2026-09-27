extends Interactable

static var _marker_material: StandardMaterial3D

var filled: bool = false
var _marker: MeshInstance3D

func _ready() -> void:
	_marker = MeshInstance3D.new()
	var box := BoxMesh.new()
	var shape = get("shape")
	box.size = shape.size if shape is BoxShape3D else Vector3.ONE * 0.3
	box.material = _get_marker_material()
	_marker.mesh = box
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
