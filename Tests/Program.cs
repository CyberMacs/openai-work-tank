using OpenAIWorkTank.Codex;

var tests = new List<(string Name, Action Run)>
{
    ("Weekly secondary is selected", () => Assert(WeeklyQuotaSelector.Select([Bucket("codex", 300, 20, 10080, 27)]).WeeklyRemaining == 73)),
    ("Codex weekly wins among buckets", () => Assert(WeeklyQuotaSelector.Select([Bucket("other", 10080, 10), Bucket("codex", 10080, 35)]).WeeklyRemaining == 65)),
    ("Five hour never becomes weekly", () => Assert(WeeklyQuotaSelector.Select([Bucket("codex", 300, 20)]).Weekly is null)),
    ("100 percent used produces zero", () => Assert(WeeklyQuotaSelector.Select([Bucket("codex", 10080, 100)]).WeeklyRemaining == 0)),
    ("Out of range usage is clamped", () => Assert(WeeklyQuotaSelector.Select([Bucket("codex", 10080, -12)]).WeeklyRemaining == 100 && WeeklyQuotaSelector.Select([Bucket("codex", 10080, 120)]).WeeklyRemaining == 0)),
    ("Missing duration is unavailable", () => Assert(WeeklyQuotaSelector.Select([new RateLimitBucket("codex", null, new RateLimitWindow(20, null, null), null)]).Weekly is null)),
};
foreach (var test in tests) { test.Run(); Console.WriteLine("PASS " + test.Name); }
Console.WriteLine($"{tests.Count} deterministic quota-selection tests passed.");

static RateLimitBucket Bucket(string id, long primaryDuration, int primaryUsed, long? secondaryDuration = null, int secondaryUsed = 0) => new(id, null, new RateLimitWindow(primaryUsed, primaryDuration, null), secondaryDuration is null ? null : new RateLimitWindow(secondaryUsed, secondaryDuration, null));
static void Assert(bool condition) { if (!condition) throw new Exception("Assertion failed."); }
