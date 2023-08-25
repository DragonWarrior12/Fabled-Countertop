extends Button

@export var entity: PackedScene

func _pressed():
	var newEntity = entity.instantiate()
	$"/root/Countertop/Entities".add_child(newEntity, true)
	newEntity.rpc("RpcSetPos", ($"/root/Countertop/Player" as Node2D).position)
