using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.NativeInterop;

public static class FileBrowser
{
    private static Node dialog;
    private static SignalAwaiter fileOpened;
    private static SignalAwaiter canceled;

    public static class filterPresets {
        public static string[] json = new string[] { "*.json ; Json Files (*.json)", "*.* ; All Files (*.*)" };
        public static string[] images = new string[] { "*.png, *.jpg, *.jpeg, *.qoi, *.bmp, *.gif, *.tiff, *.psd, *.webp ; Image Files (*.png, *.jpg, *.jpeg, *.qoi, *.bmp, *.gif, *.tiff, *.psd, *.webp)", "*.* ; All Files (*.*)" };
    }

    public static void SetDialogNode(Node _dialog)
    {
        dialog = _dialog;
        fileOpened = dialog.ToSignal(dialog, "file_selected");
        canceled = dialog.ToSignal(dialog, "canceled");
    }

    public static async Task<string> OpenFile(string[] filters = null)
    {
        dialog.Set("file_mode", 0);
        dialog.Set("filters", filters);

        dialog.Call("show");

        Variant[] result = await fileOpened;

        GD.Print(result.Count());

        return ((string)result[0]).StripEdges();
    }

    public static async Task<string> SaveFile(string[] filters = null)
    {
        dialog.Set("file_mode", 3);
        dialog.Set("filters", filters);

        dialog.Call("show");

        Variant[] result = await fileOpened;

        return ((string)result[0]).StripEdges();
    }
}
