using System.Text.Json;

namespace OpenAIWorkTank.Codex;

public sealed record RateLimitWindow(int UsedPercent, long? WindowDurationMins, long? ResetsAt);
public sealed record RateLimitBucket(string? LimitId, string? LimitName, RateLimitWindow? Primary, RateLimitWindow? Secondary);
public sealed record QuotaSelection(RateLimitWindow? Weekly, RateLimitWindow? FiveHours, string Diagnostic)
{
    public int? WeeklyRemaining => Weekly is null ? null : Math.Clamp(100 - Weekly.UsedPercent, 0, 100);
    public int? FiveHoursRemaining => FiveHours is null ? null : Math.Clamp(100 - FiveHours.UsedPercent, 0, 100);
}

public static class RateLimitParser
{
    public static IReadOnlyList<RateLimitBucket> Parse(JsonElement result)
    {
        var buckets = new List<RateLimitBucket>();
        if (result.TryGetProperty("rateLimitsByLimitId", out var byId) && byId.ValueKind == JsonValueKind.Object)
            foreach (var property in byId.EnumerateObject()) buckets.Add(ParseBucket(property.Value, property.Name));
        if (buckets.Count == 0 && result.TryGetProperty("rateLimits", out var legacy) && legacy.ValueKind == JsonValueKind.Object)
            buckets.Add(ParseBucket(legacy, null));
        return buckets;
    }

    private static RateLimitBucket ParseBucket(JsonElement item, string? fallbackId)
    {
        string? String(string name) => item.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
        return new RateLimitBucket(String("limitId") ?? fallbackId, String("limitName"), Window("primary"), Window("secondary"));
        RateLimitWindow? Window(string name)
        {
            if (!item.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Object || !value.TryGetProperty("usedPercent", out var used) || !used.TryGetInt32(out var usedValue)) return null;
            long? Number(string key) => value.TryGetProperty(key, out var n) && n.TryGetInt64(out var x) ? x : null;
            return new RateLimitWindow(usedValue, Number("windowDurationMins"), Number("resetsAt"));
        }
    }
}

public static class WeeklyQuotaSelector
{
    public const long WeeklyMinutes = 7 * 24 * 60;
    public const long FiveHourMinutes = 5 * 60;

    public static QuotaSelection Select(IEnumerable<RateLimitBucket> buckets)
    {
        var all = buckets.ToList();
        var windows = all.SelectMany(bucket => new[] { (bucket, window: bucket.Primary, slot: "primary"), (bucket, window: bucket.Secondary, slot: "secondary") })
            .Where(x => x.window is not null).Select(x => (x.bucket, window: x.window!, x.slot)).ToList();
        var weekly = windows.Where(x => x.window.WindowDurationMins == WeeklyMinutes)
            .OrderByDescending(x => CodexScore(x.bucket)).ThenBy(x => x.bucket.LimitId, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        var fiveHours = windows.Where(x => x.window.WindowDurationMins == FiveHourMinutes)
            .OrderByDescending(x => CodexScore(x.bucket)).FirstOrDefault();
        var diagnostic = string.Join("; ", windows.Select(x => $"{x.bucket.LimitId ?? "legacy"}/{x.slot}={x.window.WindowDurationMins?.ToString() ?? "unknown"}m"));
        return new QuotaSelection(weekly.window, fiveHours.window, diagnostic.Length == 0 ? "No usable rate-limit windows received." : diagnostic);
    }

    private static int CodexScore(RateLimitBucket bucket)
    {
        var text = $"{bucket.LimitId} {bucket.LimitName}";
        return text.Contains("codex", StringComparison.OrdinalIgnoreCase) ? 10 : 0;
    }
}
