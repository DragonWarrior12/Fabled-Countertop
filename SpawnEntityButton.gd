extends Button

@export var entity: PackedScene

func _pressed():
	if (multiplayer.is_server()):
		createEntity(($"/root/Countertop/Player" as Node2D).position)
	else:
		createEntity.rpc_id(1, ($"/root/Countertop/Player" as Node2D).position)

@rpc("any_peer", "reliable")
func createEntity(pos):
	var newEntity = entity.instantiate()
	$"/root/Countertop/Entities".add_child(newEntity, true)
	newEntity.rpc("SetPos", pos)
