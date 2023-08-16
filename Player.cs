using Godot;
using System;

public partial class Player : Node2D
{
    public bool dragging { get; private set;}
    public static Player singleton { get; private set; }
    static Color gridColour = new Color(0f, 0f, 0f);
    [Export]
    public Camera2D camera;
    public float zoomSpeed = 1.1f;
    public Vector2 minZoom = 0.1f * Vector2.One;
    public Vector2 maxZoom = 20f * Vector2.One;

	public override void _Ready()
	{
        singleton = this;
        dragging = false;
	}

	public override void _Process(double delta)
	{
        QueueRedraw();
	}

    public override void _Draw()
    {
        Transform2D trans = camera.GetViewportTransform().AffineInverse();
        Rect2 vp = camera.GetViewportRect().Abs();
        vp = new Rect2(trans.BasisXform(vp.Position), trans.BasisXform(vp.Size));
        vp.Position = (-vp.Size / 200).Floor() * 100 - Position.PosMod(100);
        vp.Size += 200 * Vector2.One;
        for (int x = (int)vp.Position.X; x < vp.End.X; x += 100) {
            DrawLine(new Vector2(x, vp.Position.Y), new Vector2(x, vp.End.Y), gridColour);
        }
        for (int y = (int)vp.Position.Y; y < vp.End.Y; y += 100) {
            DrawLine(new Vector2(vp.Position.X, y), new Vector2(vp.End.X, y), gridColour);
        }
    }

    public override void _Input(InputEvent _event)
    {
        if (_event is InputEventMouseButton mouseEvent) {
            if (mouseEvent.ButtonIndex == MouseButton.Middle) {
                if (!dragging && mouseEvent.Pressed)
                {
                    dragging = true;
                }

                if (dragging && !mouseEvent.Pressed)
                {
                    dragging = false;
                }
            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelUp) {
                camera.Zoom = (camera.Zoom * zoomSpeed).Clamp(minZoom, maxZoom);
            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown) {
                camera.Zoom = (camera.Zoom / zoomSpeed).Clamp(minZoom, maxZoom);
            }
        }
        else
        {
            if (_event is InputEventMouseMotion motionEvent && dragging)
            {
                Position -= camera.GetViewportTransform().AffineInverse().BasisXform(motionEvent.Relative);
            }
        }
    }
}
