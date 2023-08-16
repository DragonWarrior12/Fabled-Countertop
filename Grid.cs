using Godot;
using System;

public partial class Grid : Node2D
{
    public override void _Draw()
    {
		Vector2 offset = ((GetParent() as Node2D).Position / 100).Floor() * 100;
        Vector2 size = DisplayServer.WindowGetSize();
        for (int x = (int)offset.X; x < size.X; x += 100) {
            DrawLine(new Vector2(x, 0), new Vector2(x, size.Y), new Color(0, 255, 255));
        }
        for (int y = (int)offset.Y; y < size.Y; y += 100) {
            DrawLine(new Vector2(0, y), new Vector2(size.X, y), new Color(0, 255, 255));
        }
    }
}
