using System.Diagnostics;
using OpenAIWorkTank.Codex;
using OpenAIWorkTank.Services;

namespace OpenAIWorkTank.Tray;

public sealed class TrayController : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _autoStart;
    private readonly StatusForm _statusForm;
    private Icon? _icon;
    private QuotaSelection? _selection;
    private string _state = "Connecting…";
    private DateTimeOffset? _lastSuccessful;
    public event Func<Task>? RefreshRequested;
    public event Action? ExitRequested;
    public TrayController()
    {
        _autoStart = new ToolStripMenuItem("Start with Windows", null, (sender, _) => AutoStartService.SetEnabled(((ToolStripMenuItem)sender!).Checked)) { Checked = AutoStartService.Enabled, CheckOnClick = true };
        var menu = new ContextMenuStrip();
        menu.Items.Add("OpenAI Work Tank", null, (_, _) => ShowDetails()); menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Refresh now", null, async (_, _) => { if (RefreshRequested is not null) await RefreshRequested(); }); menu.Items.Add(_autoStart);
        menu.Items.Add("Open status/details", null, (_, _) => ShowDetails()); menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Open logs", null, (_, _) => { Directory.CreateDirectory(AppPaths.Logs); Process.Start(new ProcessStartInfo("explorer.exe", AppPaths.Logs) { UseShellExecute = true }); }); menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitRequested?.Invoke());
        _notifyIcon = new NotifyIcon { Visible = true, ContextMenuStrip = menu, Text = "OpenAI Work Tank – connecting…" };
        _notifyIcon.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) ShowDetails(); };
        _statusForm = new StatusForm(); Render();
    }
    public void Update(QuotaSelection selection) { _selection = selection; _state = selection.Weekly is null ? "Weekly quota unavailable" : "Connected"; _lastSuccessful = DateTimeOffset.Now; Render(); }
    public void SetState(string state) { _state = state; if (!state.Equals("Connected", StringComparison.OrdinalIgnoreCase)) _selection = null; Render(); }
    private void Render()
    {
        var stale = _lastSuccessful is { } last && DateTimeOffset.Now - last > TimeSpan.FromMinutes(5);
        var remaining = _selection?.WeeklyRemaining; var category = _state.Contains("not found", StringComparison.OrdinalIgnoreCase) ? "missing" : _state.Contains("auth", StringComparison.OrdinalIgnoreCase) ? "auth" : "";
        var replacement = DynamicTrayIconRenderer.Create(remaining, category, stale);
        var old = _icon; _icon = replacement; _notifyIcon.Icon = replacement; old?.Dispose();
        _notifyIcon.Text = Tooltip(remaining, _selection?.Weekly?.ResetsAt, stale);
        _statusForm.UpdateStatus(remaining, _selection?.FiveHoursRemaining, _selection?.Weekly?.ResetsAt, _state, _lastSuccessful, stale);
    }
    private string Tooltip(int? weekly, long? reset, bool stale)
    {
        if (weekly is null) return "OpenAI Work Tank\n" + _state;
        var resetText = reset is null ? "Reset: unavailable" : $"Reset: {LocalTime(reset.Value):yyyy-MM-dd HH:mm}";
        var text = $"OpenAI Work Tank\nWeekly: {weekly}% remaining\n{resetText}" + (stale ? " (stale)" : "");
        return text.Length <= 63 ? text : text[..63];
    }
    private void ShowDetails()
    {
        var area = Screen.FromPoint(Cursor.Position).WorkingArea;
        _statusForm.Location = new Point(area.Right - _statusForm.Width - 12, area.Bottom - _statusForm.Height - 12);
        _statusForm.Show(); _statusForm.Activate();
    }
    internal static DateTimeOffset LocalTime(long unixSeconds) => DateTimeOffset.FromUnixTimeSeconds(unixSeconds).ToLocalTime();
    public void Dispose() { _notifyIcon.Visible = false; _notifyIcon.Dispose(); _icon?.Dispose(); _statusForm.Dispose(); }
}

internal sealed class StatusForm : Form
{
    private readonly Label _weekly = Label("WEEKLY", 18, 18, 300, 22, 10, FontStyle.Bold, Color.FromArgb(130, 210, 255));
    private readonly Label _weeklyValue = Label("?", 18, 42, 300, 58, 36, FontStyle.Bold, Color.White);
    private readonly ProgressBar _weeklyBar = new() { Left = 18, Top = 108, Width = 300, Height = 12, Style = ProgressBarStyle.Continuous };
    private readonly Label _reset = Label("Reset: unavailable", 18, 130, 300, 22, 10, FontStyle.Regular, Color.Gainsboro);
    private readonly Label _five = Label("5 HOURS: unavailable", 18, 170, 300, 25, 12, FontStyle.Bold, Color.Gainsboro);
    private readonly Label _connection = Label("● Connecting…", 18, 215, 300, 22, 10, FontStyle.Regular, Color.Gainsboro);
    public StatusForm()
    {
        Text = "OpenAI Work Tank"; FormBorderStyle = FormBorderStyle.FixedToolWindow; ShowInTaskbar = false; StartPosition = FormStartPosition.Manual; Size = new Size(352, 282); BackColor = Color.FromArgb(18, 30, 52); ForeColor = Color.White;
        Controls.AddRange([Label("🤖  OPENAI WORK TANK", 18, 0, 300, 22, 10, FontStyle.Bold, Color.White), _weekly, _weeklyValue, _weeklyBar, _reset, _five, _connection]);
        Deactivate += (_, _) => Hide();
    }
    public void UpdateStatus(int? weekly, int? five, long? reset, string state, DateTimeOffset? updated, bool stale)
    {
        if (InvokeRequired) { BeginInvoke(() => UpdateStatus(weekly, five, reset, state, updated, stale)); return; }
        _weeklyValue.Text = weekly is null ? "Weekly quota unavailable" : $"{weekly}% remaining"; _weeklyValue.Font = new Font("Segoe UI", weekly is null ? 16 : 30, FontStyle.Bold); _weeklyBar.Value = weekly ?? 0;
        _reset.Text = reset is null ? "Reset: unavailable" : $"Reset: {TrayController.LocalTime(reset.Value):yyyy-MM-dd HH:mm}";
        _five.Text = five is null ? "5 HOURS: unavailable" : $"5 HOURS: {five}% remaining";
        _connection.Text = $"● {state}" + (stale && updated is not null ? $" (stale, last: {updated.Value:HH:mm})" : ""); _connection.ForeColor = weekly is null ? Color.Orange : Color.FromArgb(63, 220, 150);
    }
    private static Label Label(string text, int left, int top, int width, int height, float size, FontStyle style, Color color) => new() { Text = text, Left = left, Top = top, Width = width, Height = height, Font = new Font("Segoe UI", size, style), ForeColor = color, BackColor = Color.Transparent };
}
