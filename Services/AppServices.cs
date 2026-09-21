using Microsoft.Win32;

namespace OpenAIWorkTank.Services;

public static class AppPaths
{
    public static string Root => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenAIWorkTank");
    public static string Logs => Path.Combine(Root, "logs");
}

public sealed class LogService
{
    private readonly object _gate = new();
    private readonly string _path;
    public LogService()
    {
        Directory.CreateDirectory(AppPaths.Logs); Rotate();
        _path = Path.Combine(AppPaths.Logs, "openai-work-tank.log");
    }
    public void Info(string message)
    {
        lock (_gate) File.AppendAllText(_path, $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz} {message}{Environment.NewLine}");
    }
    private static void Rotate()
    {
        var path = Path.Combine(AppPaths.Logs, "openai-work-tank.log");
        if (File.Exists(path) && new FileInfo(path).Length > 2_000_000)
        {
            for (var i = 3; i >= 1; i--) { var from = path + "." + i; var to = path + "." + (i + 1); if (File.Exists(from)) File.Move(from, to, true); }
            File.Move(path, path + ".1", true);
        }
    }
}

public static class AutoStartService
{
    private const string ValueName = "OpenAIWorkTank";
    private const string KeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
    public static bool Enabled => Registry.CurrentUser.OpenSubKey(KeyPath)?.GetValue(ValueName) is string;
    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
        if (enabled) key.SetValue(ValueName, $"\"{Application.ExecutablePath}\""); else key.DeleteValue(ValueName, false);
    }
}
