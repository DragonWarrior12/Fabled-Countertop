using Godot;
using System;

public partial class MapEditMenu : Panel
{
	[Export]
	Button pickOffset, squareSize, squareCount, confirm;
	[Export]
	SpinBox offsetX, offsetY, squareField, posX, posY;
	public Map map;
	public bool picking = false;

	public override void _EnterTree()
	{
		pickOffset.Pressed += StartPickOffset;
		squareSize.Pressed += SizePressed;
		squareCount.Pressed += CountPressed;
		confirm.Pressed += ConfirmPressed;

		offsetX.ValueChanged += OffsetPosChanged; offsetY.ValueChanged += OffsetPosChanged;
		posX.ValueChanged += OffsetPosChanged; posY.ValueChanged += OffsetPosChanged;
		squareField.ValueChanged += SquareChanged;
	}

	public void Open(Map _map)
	{
		map = _map;

		offsetX.Value = map.offset.X;
		offsetY.Value = map.offset.Y;

		posX.Value = map.squarePos.X;
		posY.Value = map.squarePos.Y;

		if (map.useSize) {
			squareField.Value = map.pixelsPerSquare;
		} else {
			squareField.Value = map.image.Texture.GetSize().X / map.pixelsPerSquare;
		}

		Visible = true;
	}

	public void OffsetPosChanged(double _)
	{
		map.offset = new Vector2((float)offsetX.Value, (float)offsetY.Value);
		map.squarePos = new Vector2I((int)posX.Value, (int)posY.Value);
		map.Position = (map.squarePos * 100) - map.offset;
	}

	public void SquareChanged(double _)
	{
		if (map.useSize) {
			map.pixelsPerSquare = (float)squareField.Value;
		} else {
			map.pixelsPerSquare = map.image.Texture.GetSize().X / (float)squareField.Value;
		}

		map.UpdateScale();
	}

	public void SizePressed()
	{
		map.useSize = true;

		squareField.Value = map.pixelsPerSquare;
	}

	public void CountPressed()
	{
		map.useSize = false;

		squareField.Value = map.image.Texture.GetSize().X / map.pixelsPerSquare;
	}

	public void StartPickOffset()
	{
		picking = true;
	}

	public void Pick(Vector2 _offset)
	{
		picking = false;

		offsetX.Value = _offset.X;
		offsetY.Value = _offset.Y;

		OffsetPosChanged(0);
	}

	public void ConfirmPressed()
	{
		Visible = false;
		picking = false;
		map = null;
	}
}
