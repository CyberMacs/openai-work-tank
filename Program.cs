using System.Runtime.InteropServices;
using OpenAIWorkTank.App;

namespace OpenAIWorkTank;

internal static class Program
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)] private static extern int SetCurrentProcessExplicitAppUserModelID(string appId);
    [STAThread] static void Main()
    {
        using var mutex = new Mutex(true, "Local\\OpenAIWorkTank-0E1A0EF9-74E9-47C6-87B6-0A4F1D9EBC81", out var first);
        if (!first) return; SetCurrentProcessExplicitAppUserModelID("OpenAI.Work.Tank"); ApplicationConfiguration.Initialize(); Application.Run(new TankApplicationContext());
    }
}
