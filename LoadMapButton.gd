extends Button

@export var map: PackedScene

func _pressed():
	if !multiplayer.is_server(): return
	var newMap = map.instantiate()
	newMap.position = ($"/root/Countertop/Player" as Node2D).position
	$"/root/Countertop/Maps".add_child(newMap, true)
	newMap.image.OpenDialog()
