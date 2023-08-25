using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using ImageMagick;
using System.Security.Cryptography;
using System.Collections.Generic;

using File = System.IO.File;

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

        if (hash != null) SetImage(hash);

        if (placeholder == null) return;
        Action removePlaceholder = () => placeholder.QueueFree();
        removePlaceholder += () => Loaded -= removePlaceholder;
        Loaded += removePlaceholder;
    }

    public void AddImageSubmenu(RightClickable clickable)
    {
        PopupMenu submenu = new PopupMenu();

        submenu.Name = "ImageSubmenu";

        if (Multiplayer.IsServer()) {
            submenu.AddItem("Load", FunctionIDs.LoadImage);
            clickable.actions.Add(FunctionIDs.LoadImage, OpenDialog);
            submenu.AddItem("Tint", FunctionIDs.TintImage);
            clickable.actions.Add(FunctionIDs.TintImage, () => Rpc("RpcTintImage", colourPicker.Color));
        } else {
            submenu.AddItem("Sync", FunctionIDs.SyncImage);
            clickable.actions.Add(FunctionIDs.SyncImage, () => RpcId(source, "RpcRequestImage", hash));
        }

        clickable.AddSubmenu("Image", submenu);
    }

    public async void OpenDialog()
    {
        await Task.Run(() => {
            string path = FileBrowser.OpenFile();

            FileSelected?.Invoke(path);

            LoadFromFile(path.StripEdges());
        });
    }

    public Vector2 WorldPointToPixel(Vector2 worldPoint)
    {
        return (worldPoint - ((Node2D)GetParent()).Position + Texture.GetSize() * Scale / 2) / Scale;
    }

    public async void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        byte[] data = await Task<byte[]>.Run(() => { return File.ReadAllBytes(filePath); });
        GD.Print(data.Length);
        
        hash = hasher.ComputeHash(data).HexEncode();

        if (textures.ContainsKey(hash)) { Texture = textures[hash].texture; Loaded?.Invoke(); }
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

        Rpc("RpcSetImage", hash);
    }

    public void Load(byte[] imageData)
    {
        Image image = new Image();
        image.LoadWebpFromBuffer(imageData);
        if (!textures.ContainsKey(hash)) textures[hash] = new UserImage();
        textures[hash].texture.SetImage(image);
        textures[hash].data = imageData;
        Texture = textures[hash].texture;
        Loaded?.Invoke();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=true, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RpcTintImage(Color color)
    {
        SelfModulate = color;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RpcRequestImage(string hash)
    {
        Task.Run(() => { RpcId(Multiplayer.GetRemoteSenderId(), "RpcSendImage", hash, textures[hash].data); });
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal=false, TransferMode=MultiplayerPeer.TransferModeEnum.Reliable)]
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
        if (Multiplayer.IsServer()) return;

        SetImage(_hash);
    }

    public void SetImage(string _hash)
    {
        if (textures.ContainsKey(_hash)) {
            Texture = textures[_hash].texture;
            Loaded?.Invoke();
            return;
        }

        textures.Add(_hash, new UserImage());
        
        Texture = textures[_hash].texture;

        hash = _hash;

        RpcId(1, "RpcRequestImage", _hash);
    }
}

public class UserImage
{
    public ImageTexture texture;
    public byte[] data;

    public UserImage()
    {
        texture = new ImageTexture();
    }
}
