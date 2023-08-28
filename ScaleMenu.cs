using Godot;
using System;

public partial class ScaleMenu : Panel
{
	[Export]
	Button confirm;
	[Export]
	SpinBox scaleBox;
	public Entity ent;

	public override void _EnterTree()
	{
		confirm.Pressed += ConfirmPressed;

		scaleBox.ValueChanged += ScaleChanged;
	}

	public void Open(Entity _ent)
	{
		ent = _ent;

		scaleBox.Value = ent.size;

		Visible = true;
	}

	public void ScaleChanged(double scale)
	{
		ent.Rpc("RpcSetScale", (float)scale);
	}

	public void ConfirmPressed()
	{
		Visible = false;
		ent = null;
	}
}
