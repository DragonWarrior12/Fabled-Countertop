using Godot;
using System;

public partial class NetManager : Node
{
    public string IP = "127.0.0.1";
    public ushort port = 7777;
    // [Export]
    // PackedScene playerScene;

    public override void _Ready()
    {
        ToSignal(GetNode("%MainMenu/%HostButton"), "pressed").OnCompleted(Host);
        ToSignal(GetNode("%MainMenu/%JoinButton"), "pressed").OnCompleted(Join);

        if (!Multiplayer.IsServer()) return;

        // Multiplayer.PeerConnected += AddPlayer;
        // Multiplayer.PeerDisconnected += RemPlayer;

        // foreach (long id in Multiplayer.GetPeers()) {
        //     AddPlayer(id);
        // }

        // AddPlayer(1);
    }

    // public override void _ExitTree()
    // {
    //     base._ExitTree();

    //     Multiplayer.PeerConnected -= AddPlayer;
    //     Multiplayer.PeerDisconnected -= RemPlayer;
    // }

    public void Host()
    {
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        if (peer.CreateServer(port) != Error.Ok) return;
        Multiplayer.MultiplayerPeer = peer;
        OnServerConnect();
    }

    public void Join()
    {
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        if (peer.CreateClient(IP, port) != Error.Ok) return;
        Multiplayer.MultiplayerPeer = peer;
        OnClientConnect();
    }

    public void OnServerConnect()
    {
        OnConnect();
    }

    public void OnClientConnect()
    {
	    OnConnect();
    }

    // public void AddPlayer(long id)
    // {
    //     Node playerObj = playerScene.Instantiate();

    //     playerObj.Name = id.ToString();
    // }

    // public void RemPlayer(long id)
    // {

    // }

    public void OnConnect()
    {
	    (GetNode("/root/Countertop/%MainMenu") as CanvasItem).Visible = false;
	    (GetNode("/root/Countertop/%CountertopUI") as CanvasItem).Visible = true;
    }
}
