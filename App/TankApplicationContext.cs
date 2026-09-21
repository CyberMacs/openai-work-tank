using OpenAIWorkTank.Codex;
using OpenAIWorkTank.Services;
using OpenAIWorkTank.Tray;

namespace OpenAIWorkTank.App;

public sealed class TankApplicationContext : ApplicationContext
{
    private readonly LogService _log = new(); private readonly TrayController _tray; private readonly CodexAppServerClient _client; private readonly System.Windows.Forms.Timer _refreshTimer = new() { Interval = 60_000 }; private readonly SynchronizationContext _ui; private int _retrySeconds = 2; private bool _closing; private bool _reconnectScheduled;
    public TankApplicationContext()
    {
        _ui = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
        _log.Info("OpenAI Work Tank 0.1.2 starting."); _tray = new TrayController(); _client = new CodexAppServerClient(_log); _tray.RefreshRequested += () => _client.RefreshAsync(); _tray.ExitRequested += Exit;
        _client.QuotasUpdated += selection => _ui.Post(_ => { if (!_closing) { _retrySeconds = 2; _tray.Update(selection); } }, null);
        _client.StateChanged += state => _ui.Post(_ => OnStateChanged(state), null);
        _refreshTimer.Tick += async (_, _) => await _client.RefreshAsync(); _refreshTimer.Start(); _ = ConnectAsync();
    }
    private async Task ConnectAsync() { await _client.StartAsync(); }
    private void OnStateChanged(string state)
    {
        if (_closing) return; _tray.SetState(state);
        if (!_client.IsConnected && !_reconnectScheduled)
        {
            _reconnectScheduled = true; var wait = _retrySeconds; _retrySeconds = Math.Min(_retrySeconds switch { 2 => 5, 5 => 15, 15 => 30, _ => 60 }, 60);
            var timer = new System.Windows.Forms.Timer { Interval = wait * 1000 };
            timer.Tick += async (_, _) => { timer.Stop(); timer.Dispose(); _reconnectScheduled = false; await _client.StartAsync(); };
            timer.Start();
        }
    }
    private void Exit() { _closing = true; _refreshTimer.Stop(); _client.Dispose(); _tray.Dispose(); ExitThread(); }
}

