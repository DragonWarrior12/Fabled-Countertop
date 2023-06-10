using Godot;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public partial class Map : RightClickable
{
    [Export]
    NetImage image;
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
    
    public override void _Ready()
    {
        image.Loaded += ImageLoaded;
        image.FileSelected += FindSize;
        if (Multiplayer.IsServer()) {
            fog.Color = new Color(0, 0, 0, 0.3f);
            image.OpenDialog();
        }
        Position = (Position / 100).Round() * 100;
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

    public override void _Draw()
    {
        // Vector2 size = DisplayServer.WindowGetSize();
        // for (int x = 0; x < size.X; x += 100) {
        //     DrawLine(new Vector2(x, 0), new Vector2(x, size.Y), new Color(0, 255, 255));
        // }
        // for (int y = 0; y < size.Y; y += 100) {
        //     DrawLine(new Vector2(0, y), new Vector2(size.X, y), new Color(0, 255, 255));
        // }
    }

    public void ImageLoaded()
    {
        if (!Multiplayer.IsServer()) return;
        
        Vector2 size = image.Texture.GetSize();
        if (xSquares > 0) {
            float xScale = size.X / xSquares;
            float yScale = size.Y / ySquares;

            if (xScale != yScale) return;
            pixelsPerSquare = xScale;
        }

        // fog.Scale = size * scaleMultiplier;
        // image.Scale = Vector2.One * scaleMultiplier;

        Rpc("RpcScale", size * scaleMultiplier, Vector2.One * scaleMultiplier);
    }

    [Rpc (CallLocal=true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    void RpcScale(Vector2 fogScale, Vector2 imageScale)
    {
        fog.Scale = fogScale;
        image.Scale = imageScale;
    }

    public override void _Notification(int what)
    {
        if (what == Node.NotificationSceneInstantiated) Name = Guid.NewGuid().ToString();
        base._Notification(what);
    }
}
