using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Rzeka;
using Rzeka.Reporting;

// Sends real crash reports to a deployed Ingest function.
// Usage: RZEKA_REPORTS_URL='https://…/api/reports?code=…' dotnet run --project samples/CrashReportDemo

string? url = Environment.GetEnvironmentVariable("RZEKA_REPORTS_URL");
if (string.IsNullOrWhiteSpace(url))
{
    Console.Error.WriteLine("Set RZEKA_REPORTS_URL to the Ingest URL, including ?code=…");
    return 1;
}

var sink = new EchoSink(new HttpCrashReportSink(new Uri(url)));
var spring = new Spring();
using CrashReporting reporting = spring.EnableCrashReporting(
    new CrashReportingOptions
    {
        Sink = sink,
        AppName = "crash-report-demo",
        AppVersion = "1.0.0",
    }
);
IRzeka river = spring.Create("Demo", ImmediateScheduler.Instance);

var attacks = new Subject<GoblinAttacked>();
using var game = river.Strand(new Game(), attacks);

Console.WriteLine("1. A combat system fails while consent is still unknown.");
RunCombat(river, attacks);
Console.WriteLine($"   Consent: {reporting.Consent}. The report is held, nothing sent.\n");

Console.WriteLine("2. The player agrees to crash reports.");
reporting.SetConsent(granted: true);

Console.WriteLine("3. The combat system restarts and fails the same way (should group as one issue).");
RunCombat(river, attacks);

Console.WriteLine("4. A UI weave fails differently (should become a second issue).");
using (river.Loom<GoblinAttacked, DamageTaken>(new Combat(), ToDamage))
using (river.Weave<DamageTaken>(new DamageLabel(), d => d.Subscribe(DamageLabel.Show)))
    attacks.OnNext(new GoblinAttacked(strength: 3));

Console.WriteLine("\nWaiting for the endpoint (a cold start can take a few seconds)…");
int accepted = sink.WaitFor(expected: 3, TimeSpan.FromSeconds(45));
Console.WriteLine($"Done: {accepted} of 3 reports accepted.");
return accepted == 3 ? 0 : 2;

static void RunCombat(IRzeka river, Subject<GoblinAttacked> attacks)
{
    using var damage = river.Loom<GoblinAttacked, DamageTaken>(new Combat(), ToDamage);
    using var health = river.Loom<DamageTaken, HealthChanged>(new Health(), d => d.Select(Health.Apply));

    attacks.OnNext(new GoblinAttacked(strength: 12));
}

static IObservable<DamageTaken> ToDamage(IObservable<GoblinAttacked> attacks) =>
    attacks.Select(attack => new DamageTaken(attack.Strength * 10));

sealed class GoblinAttacked(int strength) : Matter
{
    public int Strength { get; } = strength;
}

sealed class DamageTaken(int amount) : Matter
{
    public int Amount { get; } = amount;
}

sealed class HealthChanged(int health) : Matter
{
    public int Health { get; } = health;
}

sealed class Game;

sealed class Combat;

sealed class Health
{
    // The path in the message is there to show the scrubber at work.
    public static HealthChanged Apply(DamageTaken damage) =>
        damage.Amount > 100
            ? throw new InvalidOperationException(
                $"Damage {damage.Amount} exceeds max health; see /home/{Environment.UserName}/sanctuary/combat.log"
            )
            : new HealthChanged(100 - damage.Amount);
}

sealed class DamageLabel
{
    static readonly Dictionary<string, string> Labels = new() { ["Goblin"] = "Ouch!" };

    public static void Show(DamageTaken damage) => Console.WriteLine(Labels["Dragon"]);
}

// Passes each report to the real sink, prints the outcome, and lets the demo wait for them.
sealed class EchoSink(ICrashReportSink inner) : ICrashReportSink
{
    readonly SemaphoreSlim _attempted = new(0);
    int _accepted;

    public async Task SendAsync(CrashReport report, CancellationToken cancellationToken)
    {
        try
        {
            await inner.SendAsync(report, cancellationToken);
            Interlocked.Increment(ref _accepted);
            Console.WriteLine(
                $"   → sent {report.ReportId}: {report.Failure.Spell.Title} / {report.Failure.Exception.Type}"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ {report.ReportId} failed: {ex.Message}");
            throw;
        }
        finally
        {
            _attempted.Release();
        }
    }

    // Returns how many were accepted once `expected` attempts finished, or the timeout passed.
    public int WaitFor(int expected, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow + timeout;
        for (int i = 0; i < expected; i++)
        {
            TimeSpan left = deadline - DateTime.UtcNow;
            if (left <= TimeSpan.Zero || !_attempted.Wait(left))
                break;
        }
        return Volatile.Read(ref _accepted);
    }
}
