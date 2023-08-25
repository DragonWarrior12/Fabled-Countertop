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
	// 0xx Entities
	// 1xx Images
	LoadImage = 100,
	SyncImage = 101,
	TintImage = 102,
	// 2xx Maps
	EditMap = 200;
}
