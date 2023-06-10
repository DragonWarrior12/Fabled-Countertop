using System;
using System.Diagnostics;
using System.Threading.Tasks;

public static class FileBrowser
{
    public static string OpenFile()
    {
        string command = @"Add-Type -AssemblyName System.Windows.Forms
        $FileDialog = New-Object -Typename System.Windows.Forms.OpenFileDialog
        [void]$FileDialog.ShowDialog()
        $FileDialog.Filename";

        Process dialog = new Process();

        dialog.StartInfo.FileName = "powershell.exe";
        dialog.StartInfo.Arguments = command;
        dialog.StartInfo.UseShellExecute = false;
        dialog.StartInfo.CreateNoWindow = true;
        dialog.StartInfo.RedirectStandardOutput = true;

        dialog.Start();

        string path = dialog.StandardOutput.ReadToEnd();
        dialog.WaitForExit();
        return path;
    }
}
