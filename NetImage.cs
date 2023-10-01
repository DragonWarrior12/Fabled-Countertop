using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using ImageMagick;
using System.Security.Cryptography;
using System.Collections.Generic;

using File = System.IO.File;
using Newtonsoft.Json.Linq;
using SharpFileDialog;

public partial class NetImage : Sprite2D
{
    static SHA512 hasher = SHA512.Create();
    static Dictionary<string, UserImage> textures = new Dictionary<string, UserImage>();
    static ColorPickerButton colourPicker;
    [Export]
    string hash;
    int source = 1;
    public Action Loaded;
    public Action<string> FileSelected;
    [Export]
    public Node placeholder;

    public override void _Ready()
    {
        colourPicker = GetNode("/root/Countertop/UI/CountertopUI/ColourPicker") as ColorPickerButton;

        if (placeholder == null) return;
        Action removePlaceholder = () => { if (placeholder != null) placeholder.QueueFree(); };
        removePlaceholder += () => Loaded -= removePlaceholder;
        Loaded += removePlaceholder;

        if (hash != "") SetImage(hash);
    }

    public void AddImageSubmenu(RightClickable clickable)
    {
        PopupMenu submenu = new PopupMenu();

        submenu.Name = "ImageSubmenu";

        if (NetManager.isServer) {
            submenu.AddItem("Load", FunctionIDs.LoadImage);
            clickable.actions.Add(FunctionIDs.LoadImage, OpenDialog);
            submenu.AddItem("Tint", FunctionIDs.TintImage);
            clickable.actions.Add(FunctionIDs.TintImage, () => MainThreadInvoker.InvokeOnMainThread(() => { Rpc("RpcTintImage", colourPicker.Color); }));
        } else {
            submenu.AddItem("Sync", FunctionIDs.SyncImage);
            clickable.actions.Add(FunctionIDs.SyncImage, () => MainThreadInvoker.InvokeOnMainThread(() => { RpcId(source, "RpcRequestImage", hash); }));
        }

        clickable.AddSubmenu("Image", submenu);
    }

    public async void OpenDialog()
    {
        await Task.Run(async () => {
            if (!NativeFileDialog.OpenDialog(null, null, out string path)) return;

            FileSelected?.Invoke(path);

            await LoadFromFile(path);
        });
    }

    public Vector2 WorldPointToPixel(Vector2 worldPoint)
    {
        return (worldPoint - ((Node2D)GetParent()).Position + Texture.GetSize() * Scale / 2) / Scale;
    }

    public async Task LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        byte[] data = await Task<byte[]>.Run(() => { return File.ReadAllBytes(filePath); });
        
        hash = hasher.ComputeHash(data).HexEncode();

        if (textures.ContainsKey(hash)) MainThreadInvoker.InvokeOnMainThread(() => { Texture = textures[hash].texture; textures[hash].Loaded += Loaded.Invoke; Loaded?.Invoke(); });
        else {
            if (await Task<bool>.Run(() => { 
                try {
                    MagickImage image = new MagickImage(data);
                    image.Format = MagickFormat.WebP;
                    data = image.ToByteArray();
                    image.Dispose();
                } catch { return true; }
                return false;
            })) return;

            await Task.Run(() => Load(data));
        }

        textures[hash].path = filePath;

        MainThreadInvoker.InvokeOnMainThread(() => { Rpc("RpcSetImage", hash); });
    }

    public void Load(byte[] imageData)
    {
        Image image = new Image();
        image.LoadWebpFromBuffer(imageData);
        if (!textures.ContainsKey(hash)) textures[hash] = new UserImage();

        MainThreadInvoker.InvokeOnMainThread(() => {
            Texture = textures[hash].texture;
            textures[hash].Loaded += Loaded.Invoke;
            textures[hash].SetImage(image);
            textures[hash].data = imageData;
        });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=true, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RpcTintImage(Color color)
    {
        SelfModulate = color;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RpcRequestImage(string hash)
    {
        MainThreadInvoker.InvokeOnMainThread(() => { RpcId(Multiplayer.GetRemoteSenderId(), "RpcSendImage", hash, textures[hash].data); });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 1)]
    public void RpcSendImage(string _hash, byte[] _data)
    {
        Task.Run(() => {
            hash = _hash;
            Load(_data);
        });
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RpcSetImage(string _hash)
    {
        if (NetManager.isServer) return;

        SetImage(_hash);
    }

    public void SetImage(string _hash)
    {
        if (textures.ContainsKey(_hash)) {
            Texture = textures[_hash].texture;
            textures[hash].Loaded += Loaded.Invoke;
            Loaded?.Invoke();
            return;
        }

        textures.Add(_hash, new UserImage());
        
        Texture = textures[_hash].texture;

        textures[hash].Loaded += Loaded.Invoke;

        hash = _hash;

        MainThreadInvoker.InvokeOnMainThread(() => { RpcId(1, "RpcRequestImage", _hash); });
    }

    public JObject SaveJson()
    {
        return new JObject
        {
            { "Path", textures[hash].path },
            { "Tint", SelfModulate.ToHtml() }
        };
    }

    public async void LoadJson(JObject _data)
    {
        if (_data.ContainsKey("Path") && _data["Path"].Type == JTokenType.String) {
            string path = (string)_data["Path"];
            await LoadFromFile(path);
        }
        if (_data.ContainsKey("Tint") && _data["Tint"].Type == JTokenType.String) {
            SelfModulate = Color.FromHtml((string)_data["Tint"]);
        }
    }
}

public class UserImage
{
    public Action Loaded;
    public ImageTexture texture;
    public byte[] data;
    public string path;

    public void SetImage(Image _image)
    {
        texture.SetImage(_image);
        Loaded?.Invoke();
    }

    public UserImage()
    {
        texture = new ImageTexture();
    }
}
