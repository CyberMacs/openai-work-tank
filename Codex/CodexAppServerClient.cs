using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using OpenAIWorkTank.Services;

namespace OpenAIWorkTank.Codex;

public sealed class CodexAppServerClient : IDisposable
{
    private readonly LogService _log;
    private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonElement>> _pending = new();
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly CancellationTokenSource _stop = new();
    private readonly object _stateGate = new();
    private Process? _process;
    private long _nextId;
    private bool _disposed;
    private bool _starting;
    public event Action<QuotaSelection>? QuotasUpdated;
    public event Action<string>? StateChanged;
    public bool IsConnected { get; private set; }

    public CodexAppServerClient(LogService log) => _log = log;

    public async Task StartAsync()
    {
        Process? previous;
        lock (_stateGate)
        {
            if (_disposed || _starting || IsProcessRunning(_process)) return;
            _starting = true;
            previous = _process;
            _process = null;
        }
        previous?.Dispose();
        StateChanged?.Invoke("Connecting to Codex App Server…");
        try
        {
            var codexExecutable = ResolveCodexExecutable();
            _log.Info("Using Codex executable: " + codexExecutable);
            var psi = new ProcessStartInfo(codexExecutable, "app-server --stdio") { UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            process.Exited += (_, _) => HandleDisconnected(process, "Codex App Server stopped.");
            if (!process.Start()) throw new InvalidOperationException("Codex App Server could not be started.");
            lock (_stateGate)
            {
                if (_disposed) { process.Kill(true); process.Dispose(); return; }
                _process = process;
            }
            _log.Info("Codex App Server started.");
            _ = Task.Run(() => ReadOutputAsync(process, _stop.Token));
            _ = Task.Run(() => ReadErrorsAsync(process, _stop.Token));
            await RequestAsync("initialize", new { clientInfo = new { name = "openai_work_tank", title = "OpenAI Work Tank", version = "0.1.1" } }, _stop.Token);
            await NotifyAsync("initialized", new { });
            IsConnected = true; StateChanged?.Invoke("Connected");
            await RefreshAsync();
        }
        catch (Exception ex) { HandleDisconnected(null, DescribeError(ex)); }
        finally { lock (_stateGate) _starting = false; }
    }

    public async Task RefreshAsync()
    {
        if (!IsConnected) return;
        try { Apply(await RequestAsync("account/rateLimits/read", null, _stop.Token)); }
        catch (Exception ex) { HandleDisconnected(null, DescribeError(ex)); }
    }

    private async Task<JsonElement> RequestAsync(string method, object? parameters, CancellationToken cancellationToken)
    {
        var id = Interlocked.Increment(ref _nextId); var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_pending.TryAdd(id, tcs)) throw new InvalidOperationException("Duplicate request ID.");
        try { await SendAsync(new { method, id, @params = parameters }, cancellationToken); return await tcs.Task.WaitAsync(TimeSpan.FromSeconds(20), cancellationToken); }
        finally { _pending.TryRemove(id, out _); }
    }
    private Task NotifyAsync(string method, object parameters) => SendAsync(new { method, @params = parameters }, _stop.Token);
    private async Task SendAsync(object payload, CancellationToken token)
    {
        Process process;
        lock (_stateGate) process = _process ?? throw new InvalidOperationException("App Server is not running.");
        await _writeLock.WaitAsync(token);
        try { await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(payload)); await process.StandardInput.FlushAsync(); }
        finally { _writeLock.Release(); }
    }
    private async Task ReadOutputAsync(Process process, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && await process.StandardOutput.ReadLineAsync(token) is { } line)
            {
                try
                {
                    using var doc = JsonDocument.Parse(line); var root = doc.RootElement;
                    if (root.TryGetProperty("id", out var idValue) && idValue.TryGetInt64(out var id) && _pending.TryGetValue(id, out var tcs))
                    {
                        if (root.TryGetProperty("result", out var result)) tcs.TrySetResult(result.Clone());
                        else tcs.TrySetException(new InvalidOperationException(root.TryGetProperty("error", out var error) ? error.ToString() : "Unknown App Server error."));
                    }
                    else if (root.TryGetProperty("method", out var method) && method.GetString() == "account/rateLimits/updated")
                    {
                        _log.Info("Rate-limit update received; requesting a full snapshot."); _ = RefreshAsync();
                    }
                }
                catch (JsonException ex) { _log.Info("Ignoring malformed App Server JSON: " + ex.Message); }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { HandleDisconnected(process, DescribeError(ex)); }
    }
    private async Task ReadErrorsAsync(Process process, CancellationToken token)
    {
        try { while (!token.IsCancellationRequested && await process.StandardError.ReadLineAsync(token) is { } line) _log.Info("App Server stderr: " + Redact(line)); }
        catch (OperationCanceledException) { }
    }
    private void Apply(JsonElement result)
    {
        var selection = WeeklyQuotaSelector.Select(RateLimitParser.Parse(result));
        _log.Info("Rate-limit buckets: " + selection.Diagnostic + (selection.Weekly is null ? "; weekly unavailable." : "; weekly selected."));
        QuotasUpdated?.Invoke(selection);
    }
    private void HandleDisconnected(Process? source, string message)
    {
        lock (_stateGate)
        {
            if (_disposed || (source is not null && !ReferenceEquals(source, _process))) return;
            IsConnected = false;
        }
        _log.Info(message); StateChanged?.Invoke(message);
    }
    private static bool IsProcessRunning(Process? process)
    {
        if (process is null) return false;
        try { return !process.HasExited; }
        catch (InvalidOperationException) { return false; }
    }
    private static string ResolveCodexExecutable()
    {
        var desktopBin = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenAI", "Codex", "bin");
        try
        {
            if (Directory.Exists(desktopBin))
            {
                var desktopCli = Directory.EnumerateFiles(desktopBin, "codex.exe", SearchOption.AllDirectories)
                    .OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
                if (desktopCli is not null) return desktopCli;
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (IOException) { }
        return "codex"; // use PATH when Codex Desktop is not installed
    }    private static string DescribeError(Exception ex) => ex is System.ComponentModel.Win32Exception ? "Codex CLI not found. Install Codex CLI and sign in." : "App Server unavailable: " + Redact(ex.Message);
    private static string Redact(string text) => text.Replace("Bearer ", "Bearer [redacted] ", StringComparison.OrdinalIgnoreCase);
    public void Dispose()
    {
        Process? process;
        lock (_stateGate) { _disposed = true; process = _process; _process = null; }
        _stop.Cancel();
        try { if (IsProcessRunning(process)) process!.Kill(true); } catch { }
        process?.Dispose(); _writeLock.Dispose(); _stop.Dispose();
    }
}

