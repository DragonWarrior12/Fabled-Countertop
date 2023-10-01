using Godot;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

public partial class Map : RightClickable
{
    [Export]
    public NetImage image;
    [Export]
    Polygon2D fog;
    private float p_pPS = 100;
    public float pixelsPerSquare {
        get { return p_pPS; }
        set {
            p_pPS = value;
            scaleMultiplier = 100 / p_pPS;
        }
    }
    public float scaleMultiplier = 1;
    float xSquares = -1;
    float ySquares = -1;
    public Vector2I squarePos;
    public Vector2 offset;
    MapEditMenu mapEditMenu;
    public bool useSize = true;
    
    public override void _Ready()
    {
        image.Loaded += ImageLoaded;
        image.FileSelected += FindSize;
        if (NetManager.isServer) {
            fog.Color = new Color(0, 0, 0, 0.3f);
            //image.OpenDialog();
            Position = (Position / 100).Round() * 100;
        }
        
        mapEditMenu = GetNode("/root/Countertop/UI/CountertopUI/MapEditMenu") as MapEditMenu;

        if (NetManager.isServer) {
            menu.AddItem("Edit", FunctionIDs.EditMap);
            actions.Add(FunctionIDs.EditMap, () => mapEditMenu.Open(this));
        }
        
		image.AddImageSubmenu(this);
    }

    public void FindSize(string path)
    {
        Match match = Regex.Match(path, @"(\d+)\ *x\ *(\d+)");
        if (match.Success) {
            xSquares = int.Parse(match.Groups[1].Value);
            ySquares = int.Parse(match.Groups[2].Value);
        }
    }

    public override void _Process(double delta)
    {
        
    }

    public Vector2 WorldPointToSquare(Vector2 worldPoint)
    {
        return (image.WorldPointToPixel(worldPoint) / pixelsPerSquare).Floor();
    }

    public override void _Input(InputEvent _event)
    {
		if (image.MakeInputLocal(_event) is InputEventMouseButton mouseEvent) {
            if (!image.IsPixelOpaque(mouseEvent.Position)) return;
            if (mouseEvent.ButtonIndex == MouseButton.Right) {
				menu.Position = (Vector2I)mouseEvent.GlobalPosition;
				menu.Show();
				GetViewport().SetInputAsHandled();
			}
            if (mapEditMenu.map == this && mapEditMenu.picking && mouseEvent.ButtonIndex == MouseButton.Left) {
				mapEditMenu.Pick((Vector2I)(mouseEvent.Position * 100 / pixelsPerSquare));
				GetViewport().SetInputAsHandled();
			}
		}
    }

    public void ImageLoaded()
    {
        Vector2 size = image.Texture.GetSize();
        if (xSquares > 0) {
            float xScale = size.X / xSquares;
            float yScale = size.Y / ySquares;

            if (xScale == yScale)
                pixelsPerSquare = xScale;
        }

        UpdateScale();
    }

    public void UpdateScale()
    {
        MainThreadInvoker.InvokeOnMainThread(() => { Rpc("RpcScale", image.Texture.GetSize() * scaleMultiplier, Vector2.One * scaleMultiplier); });
    }

    public void UpdatePos()
    {
		MainThreadInvoker.InvokeOnMainThread(() => { Rpc("RpcSetPos", (squarePos * 100) - offset); });
    }

    [Rpc (MultiplayerApi.RpcMode.AnyPeer, CallLocal=true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    void RpcScale(Vector2 fogScale, Vector2 imageScale)
    {
        fog.Scale = fogScale;
        image.Scale = imageScale;
    }

    [Rpc (MultiplayerApi.RpcMode.AnyPeer, CallLocal=true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    void RpcSetPos(Vector2 pos)
    {
        Position = pos;
    }

    public JObject SaveJson()
    {
        return new JObject
        {
            { "Pos", JsonUtil.EncodeVector(squarePos) },
            { "Offset", JsonUtil.EncodeVector(offset) },
            { "PixelsPerSquare", pixelsPerSquare },
            { "UseSize", useSize },
            { "Image", image.SaveJson() }
        };
    }

    public void LoadJson(JObject _data)
    {
        if (_data.ContainsKey("Pos")) {
            squarePos = (Vector2I)JsonUtil.DecodeVector((string)_data["Pos"]);
        }
        if (_data.ContainsKey("Offset")) {
            offset = JsonUtil.DecodeVector((string)_data["Offset"]);
        }
        UpdatePos();
        if (_data.ContainsKey("PixelsPerSquare"))
        {
            pixelsPerSquare = (int)_data["PixelsPerSquare"];
        }
        UpdateScale();
        if (_data.ContainsKey("UseSize"))
        {
            useSize = (bool)_data["UseSize"];
        }
        if (_data.ContainsKey("Image")) {
            image.LoadJson((JObject)_data["Image"]);
        }
    }

    public override void _Notification(int what)
    {
        if (what == Node.NotificationSceneInstantiated) Name = Guid.NewGuid().ToString();
        base._Notification(what);
    }
}
