using Godot;
using System;

public partial class Player : Node2D
{
    public bool dragging { get; private set;}
    public static Player singleton { get; private set; }

	public override void _Ready()
	{
        singleton = this;
        dragging = false;
	}

	public override void _Process(double delta)
	{

	}

    public override void _Input(InputEvent _event)
    {
        if (_event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Middle)
        {
			if (!dragging && mouseEvent.Pressed)
			{
				dragging = true;
			}

            if (dragging && !mouseEvent.Pressed)
            {
                dragging = false;
            }
        }
        else
        {
            if (_event is InputEventMouseMotion motionEvent && dragging)
            {
                Position -= motionEvent.Relative;
            }
        }
    }
}
