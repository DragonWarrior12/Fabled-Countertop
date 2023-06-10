extends Button

func _pressed():
	($"/root/Countertop/%MainMenu" as CanvasItem).visible = false
	($"/root/Countertop/%CountertopUI" as CanvasItem).visible = true
