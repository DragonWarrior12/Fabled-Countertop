using Godot;
using System;
using System.Collections.Generic;

public partial class RightClickable : Node2D
{
	[Export]
	public PopupMenu menu;
	public Dictionary<int, Action> actions = new Dictionary<int, Action>();

	public override void _EnterTree()
	{
		menu.IdPressed += ClickItem;
	}

	public void AddSubmenu(string text, PopupMenu submenu)
	{
		submenu.IdPressed += ClickItem;
        menu.AddChild(submenu);
		menu.AddSubmenuItem(text, submenu.Name);
	}

	public void ClickItem(long _id)
	{
		int id = (int)_id;
		if (actions.ContainsKey(id))
		{
			actions[id].Invoke();
		}
	}
}

public static class FunctionIDs
{
	public static int
	// 0x Entities
	ScaleEnitity = 00,
	LockEntity = 01, // need to implement, synced bool allowing/disallowing client to move
	DisableEntity = 02, // need to implement, image opacity based on bool + authority
	DupeEntity = 03, // need to implement
	// 1x Images
	LoadImage = 10,
	SyncImage = 11,
	TintImage = 12,
	// 2x Maps
	EditMap = 20;
}
