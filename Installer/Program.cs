using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;

namespace OpenAIWorkTankInstaller;

internal static class Program
{
    [STAThread] static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            var target = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenAIWorkTank");
            Directory.CreateDirectory(target);
            var executable = Path.Combine(target, "OpenAIWorkTank.exe");
            using var source = typeof(Program).Assembly.GetManifestResourceStream("OpenAIWorkTank.exe") ?? throw new InvalidOperationException("A telepítő alkalmazásfájlja hiányzik.");
            using (var destination = File.Create(executable)) source.CopyTo(destination);
            using (var key = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run")) key.SetValue("OpenAIWorkTank", $"\"{executable}\"");
            Process.Start(new ProcessStartInfo(executable) { UseShellExecute = true });
            MessageBox.Show($"Az OpenAI Work Tank telepítése elkészült.\n\nHelye: {target}\n\nAz automatikus indulás bekapcsolva.", "OpenAI Work Tank", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show("A telepítés nem sikerült:\n" + ex.Message, "OpenAI Work Tank", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}


