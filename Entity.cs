using Godot;
using System;

public partial class Entity : RightClickable
{
    public bool dragging { get; private set;}
	[Export]
	NetImage image;

	public override void _Ready()
	{
		image.AddImageSubmenu(this);
	}

	public override void _Process(double delta)
	{
		
	}

	public override void _Input(InputEvent _event)
	{
		if (MakeInputLocal(_event) is InputEventMouseButton mouseEvent) {
			if (mouseEvent.ButtonIndex == MouseButton.Left) {
				if (!dragging && mouseEvent.Pressed)
				{
					if (!image.IsPixelOpaque(mouseEvent.Position)) return;
					dragging = true;
					GetViewport().SetInputAsHandled();
				}

				if (dragging && !mouseEvent.Pressed)
				{
					dragging = false;
					Position = ((Position - Scale / 2) / 100).Round() * 100 - (Vector2.One * (Scale.X - 1));
					GetViewport().SetInputAsHandled();
				}
			} else if (mouseEvent.ButtonIndex == MouseButton.Right) {
				if (!image.IsPixelOpaque(mouseEvent.Position)) return;
				menu.Position = (Vector2I)mouseEvent.GlobalPosition;
				menu.Show();
				GetViewport().SetInputAsHandled();
			}
		}
		else
		{
			if (_event is InputEventMouseMotion motionEvent && dragging && !Player.singleton.dragging)
			{
				Position += motionEvent.Relative;
			}
		}
	}

    public Vector2 Snap(Vector2 _pos)
    {
		Vector2 radiusVec = (Scale.X / 2 + 0.5f) * Vector2.One;
		return (((Position / 100) - radiusVec).Round() + radiusVec) * 100;
    }

	public override void _Notification(int what)
	{
		if (what == Node.NotificationSceneInstantiated) Name = Guid.NewGuid().ToString();
		base._Notification(what);
	}
}

public partial class Attributes
{
	public string name;
	public int health;
	public int maxHealth;
	public int tempHealth;

}
