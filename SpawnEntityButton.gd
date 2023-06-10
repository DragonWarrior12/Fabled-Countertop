extends Button

@export var entity: PackedScene

func _pressed():
	var newEntity = entity.instantiate()
	newEntity.position = ($"/root/Countertop/Player" as Node2D).position
	$"/root/Countertop/Entities".add_child(newEntity, true)
