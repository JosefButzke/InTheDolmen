extends Node

# Nodes in this group get written to the save file (scene path + transform)
# and are re-instantiated into the main scene on the next launch.
const PERSIST_GROUP := "persist"
const SAVE_DIR := "res://saves"
const SAVE_FILE := "savegame.json"
const MAIN_SCENE_PATH := "res://scenes/main.tscn"

# res:// is read-only in exported builds, so fall back to user:// there
var save_path: String:
	get:
		var dir := SAVE_DIR if OS.has_feature("editor") else "user://saves"
		return dir.path_join(SAVE_FILE)

func _ready() -> void:
	# autoloads are ready before the main scene is added, so wait a frame
	load_game.call_deferred()

func save_game() -> void:
	var scene_root := get_tree().current_scene
	if scene_root == null or scene_root.scene_file_path != MAIN_SCENE_PATH:
		return

	var nodes: Array = []
	for node in get_tree().get_nodes_in_group(PERSIST_GROUP):
		if not node is Node3D or node.scene_file_path.is_empty():
			continue
		nodes.append({
			"scene": node.scene_file_path,
			"parent": str(scene_root.get_path_to(node.get_parent())),
			"transform": var_to_str(node.global_transform),
		})

	DirAccess.make_dir_recursive_absolute(save_path.get_base_dir())
	var file := FileAccess.open(save_path, FileAccess.WRITE)
	if file == null:
		push_error("Failed to open save file: " + str(FileAccess.get_open_error()))
		return
	file.store_string(JSON.stringify({"version": 1, "nodes": nodes}, "\t"))

func load_game() -> void:
	var scene_root := get_tree().current_scene
	if scene_root == null or scene_root.scene_file_path != MAIN_SCENE_PATH:
		return
	if not FileAccess.file_exists(save_path):
		return

	var data = JSON.parse_string(FileAccess.get_file_as_string(save_path))
	if not data is Dictionary:
		push_error("Save file is corrupted: " + save_path)
		return

	for entry in data.get("nodes", []):
		var packed := load(entry["scene"]) as PackedScene
		var parent := scene_root.get_node_or_null(NodePath(entry["parent"]))
		if packed == null or parent == null:
			push_warning("Skipping saved node: " + str(entry))
			continue
		var node: Node3D = packed.instantiate()
		node.add_to_group(PERSIST_GROUP)
		parent.add_child(node)
		node.global_transform = str_to_var(entry["transform"])

func delete_save() -> void:
	if FileAccess.file_exists(save_path):
		DirAccess.remove_absolute(save_path)
