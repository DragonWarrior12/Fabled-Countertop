using Godot;
using System;

public partial class NetManager : Node
{
    public string IP = "127.0.0.1";
    public ushort port = 7777;
    // [Export]
    // PackedScene playerScene;
    LineEdit IPField;
    LineEdit PortField;

    public override void _Ready()
    {
        ToSignal(GetNode("%MainMenu/%HostButton"), "pressed").OnCompleted(Host);
        ToSignal(GetNode("%MainMenu/%JoinButton"), "pressed").OnCompleted(Join);
        IPField = GetNode("%MainMenu/%IPField") as LineEdit;
        PortField = GetNode("%MainMenu/%PortField") as LineEdit;

        IPField.FocusExited += IPEndEdit;

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

#region TextFields
    public void IPEndEdit()
    {
        if (IPField.Text.IsValidIPAddress()) {
            IP = IPField.Text;
        } else {
            IPField.Text = IP;
        }
    }

    public void PortEndEdit()
    {
        if (ushort.TryParse(PortField.Text, out ushort p) && p != 0) {
            port = p;
        } else {
            PortField.Text = port.ToString();
        }
    }
#endregion

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
