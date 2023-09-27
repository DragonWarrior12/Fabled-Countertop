using Godot;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using FileAccess = Godot.FileAccess;
using File = System.IO.File;

public partial class NetManager : Node
{
    public string IP = "127.0.0.1";
    public ushort port = 7777;
    public static bool isServer;
    // [Export]
    // PackedScene playerScene;
    LineEdit IPField;
    LineEdit PortField;
    [Export]
    PackedScene EntityPrefab, MapPrefab;

    public override void _Ready()
    {
        FileBrowser.SetDialogNode(GetNode("NativeFileDialog"));

        ToSignal(GetNode("%MainMenu/%HostButton"), "pressed").OnCompleted(Host);
        ToSignal(GetNode("%MainMenu/%JoinButton"), "pressed").OnCompleted(Join);
        (GetNode("/root/Countertop/UI/CountertopUI/EscMenu/ExitButton") as Button).Pressed += Leave;
        (GetNode("/root/Countertop/UI/CountertopUI/EscMenu/SaveButton") as Button).Pressed += Save;
        (GetNode("/root/Countertop/UI/CountertopUI/EscMenu/LoadButton") as Button).Pressed += () => Task.Run(() => { Load(); });

        IPField = GetNode("%MainMenu/%IPField") as LineEdit;
        PortField = GetNode("%MainMenu/%PortField") as LineEdit;

        IPField.FocusExited += IPEndEdit;

        Multiplayer.ServerDisconnected += OnDisconnect;
        Multiplayer.ConnectionFailed += OnConnectFail;

        //if (!NetManager.isServer) return;

        // Multiplayer.PeerConnected += AddPlayer;
        // Multiplayer.PeerDisconnected += RemPlayer;

        // foreach (long id in Multiplayer.GetPeers()) {
        //     AddPlayer(id);
        // }

        // AddPlayer(1);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        MainThreadInvoker.ProcessMainThreadQueue(delta);
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
        isServer = true;
        OnServerConnect();
    }

    public void Join()
    {
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        if (peer.CreateClient(IP, port) != Error.Ok) return;
        Multiplayer.MultiplayerPeer = peer;
        OnClientConnect();
    }

    public void Leave()
    {
        Multiplayer.MultiplayerPeer = null;
        isServer = false;
        OnDisconnect();
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

    public void Save()
    {
        JObject data = new JObject();

        JArray entities = new JArray();
        foreach (Node node in GetNode("%Entities").GetChildren())
        {
            if (node is Entity ent) {
                entities.Add(ent.SaveJson());
            }
        }

        data.Add("Entities", entities);

        JArray maps = new JArray();
        foreach (Node node in GetNode("%Maps").GetChildren())
        {
            if (node is Map map) {
                maps.Add(map.SaveJson());
            }
        }

        data.Add("Maps", maps);

        File.WriteAllText(FileBrowser.SaveFile(filters: FileBrowser.filterPresets.json).Result, data.ToString());
    }

    public void Load()
    {
        JObject data = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(
            File.ReadAllText(
                FileBrowser.OpenFile(filters: FileBrowser.filterPresets.json).Result
            )
        );

        if (data.ContainsKey("Entities") && data["Entities"] is JArray entities) {
            GD.Print("Ents");
            foreach (JObject ent in entities) {
                MainThreadInvoker.InvokeOnMainThread(() => {
                    Entity temp = EntityPrefab.Instantiate() as Entity;
                    GetNode("%Entities").AddChild(temp);
                    temp.LoadJson(ent);
                });
            }
        }

        if (data.ContainsKey("Maps") && data["Maps"] is JArray maps) {
            GD.Print("Maps");
            foreach (JObject map in maps) {
                MainThreadInvoker.InvokeOnMainThread(() => {
                    Map temp = MapPrefab.Instantiate() as Map;
                    GetNode("%Maps").AddChild(temp);
                    temp.LoadJson(map);
                });
            }
        }
    }

    public void OnConnect()
    {
	    (GetNode("/root/Countertop/%MainMenu") as CanvasItem).Visible = false;
	    (GetNode("/root/Countertop/%CountertopUI") as CanvasItem).Visible = true;
    }

    public void OnConnectFail()
    {
        Multiplayer.MultiplayerPeer = null;
        OnDisconnect();
    }

    public void OnDisconnect()
    {
        foreach (Node child in GetNode("%Entities").GetChildren() + GetNode("%Maps").GetChildren()) {
            child.QueueFree();
        }
        
	    (GetNode("/root/Countertop/%MainMenu") as CanvasItem).Visible = true;
	    (GetNode("/root/Countertop/%CountertopUI") as CanvasItem).Visible = false;
	    (GetNode("/root/Countertop/%CountertopUI/EscMenu") as CanvasItem).Visible = false;
    }
}
