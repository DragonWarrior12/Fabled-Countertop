using Godot;
using System;
using Newtonsoft.Json.Linq;

public partial class Entity : RightClickable
{
    public bool dragging { get; private set;}
	[Export]
	NetImage image;
	[Export]
	public float size = 1;
	ScaleMenu scaleMenu;

	public override void _Ready()
	{
		scaleMenu = GetNode("/root/Countertop/UI/CountertopUI/ScaleMenu") as ScaleMenu;

        menu.AddItem("Scale", FunctionIDs.ScaleEnitity);
        actions.Add(FunctionIDs.ScaleEnitity, () => scaleMenu.Open(this));

		image.AddImageSubmenu(this);
		image.Loaded += ScaleImage;
	}

	public override void _Process(double delta)
	{
		
	}

	public void ScaleImage()
	{
		image.Scale = image.Texture.GetSize().Inverse() * 100 * size;
	}

	public override void _Input(InputEvent _event)
	{
		if (image.MakeInputLocal(_event) is InputEventMouseButton mouseEvent) {
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
					if (!Input.IsKeyPressed(Key.Shift)) Position = Snap(Position);
					MainThreadInvoker.InvokeOnMainThread(() => { Rpc("RpcSetPos", Position); });
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
				Position += GetViewportTransform().AffineInverse().BasisXform(motionEvent.Relative);
			}
		}
	}

    public Vector2 Snap(Vector2 _pos)
    {
		Vector2 radiusVec = (Scale.X / 2 + 0.5f) * Vector2.One;
		return (((Position / 100) - radiusVec).Round() + radiusVec) * 100;
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RpcSetPos(Vector2 pos)
	{
		Position = pos;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RpcSetScale(float scale)
	{
		size = scale;
		ScaleImage();
	}

	public override void _Notification(int what)
	{
		if (what == Node.NotificationSceneInstantiated) Name = Guid.NewGuid().ToString();
		base._Notification(what);
	}

    public JObject SaveJson()
    {
        return new JObject
        {
            { "Size", size },
            { "Pos", JsonUtil.EncodeVector(Position) },
            { "Image", image.SaveJson() }
        };
    }

    public void LoadJson(JObject _data)
    {
        if (_data.ContainsKey("Pos")) {
			MainThreadInvoker.InvokeOnMainThread(() => { Rpc("RpcSetPos", JsonUtil.DecodeVector((string)_data["Pos"])); });
        }
        if (_data.ContainsKey("Size")) {
            MainThreadInvoker.InvokeOnMainThread(() => { Rpc("RpcSetScale", (float)_data["Size"]); });
        }
        if (_data.ContainsKey("Image")) {
            image.LoadJson((JObject)_data["Image"]);
        }
    }
}
