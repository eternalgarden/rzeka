using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Rzeka.Reporting;

namespace Rzeka.Tests.Reporting;

public class CrashReportingTests
{
    sealed class Ping : Matter { }

    sealed class Pong : Matter { }

    sealed class Secretive
    {
        public override string ToString() => "dear diary";
    }

    internal sealed class FakeSink : ICrashReportSink
    {
        public readonly BlockingCollection<CrashReport> Received = new();

        public Task SendAsync(CrashReport report, CancellationToken cancellationToken)
        {
            Received.Add(report, cancellationToken);
            return Task.CompletedTask;
        }

        public CrashReport Next() =>
            Received.TryTake(out CrashReport? report, TimeSpan.FromSeconds(5))
                ? report
                : throw new TimeoutException("No report arrived.");

        public bool NothingMoreWithin(TimeSpan wait) => !Received.TryTake(out _, wait);
    }

    sealed class FailingSink : ICrashReportSink
    {
        public int Attempts;

        public Task SendAsync(CrashReport report, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Attempts);
            throw new HttpRequestException("network down");
        }
    }

    internal static CrashReportingOptions Options(
        ICrashReportSink sink,
        int max = 10,
        bool labels = false,
        ReportingConsent consent = ReportingConsent.Granted
    ) =>
        new()
        {
            AppName = "tests",
            AppVersion = "1.2.3",
            Sink = sink,
            MaxReportsPerSession = max,
            IncludeOwnerLabels = labels,
            Consent = consent,
        };

    [Fact]
    public void Failing_loom_is_reported_with_its_trigger_chain_and_breadcrumbs()
    {
        var spring = new Spring();
        var sink = new FakeSink();
        using var reporting = spring.EnableCrashReporting(Options(sink));
        IRzeka river = spring.Create("test", ImmediateScheduler.Instance);

        var pings = new Subject<Ping>();
        using var strand = river.Strand("game", pings);
        using var loom = river.Loom<Ping, Pong>(
            "combat",
            p => p.Select<Ping, Pong>(_ => throw new InvalidOperationException("boom"))
        );

        var ping = new Ping();
        pings.OnNext(ping);
        CrashReport report = sink.Next();

        Assert.Equal(CrashReport.CurrentSchemaVersion, report.SchemaVersion);
        Assert.Equal("tests", report.App.Name);
        Assert.Equal("Looming of Ping into Pong", report.Failure.Spell.Title);
        Assert.Equal("Looming", report.Failure.Spell.School);
        Assert.Equal("String", report.Failure.Spell.OwnerType);
        Assert.Equal("System.InvalidOperationException", report.Failure.Exception.Type);
        Assert.Equal("boom", report.Failure.Exception.Message);
        Assert.Equal(new[] { ping.Guid }, report.Triggers);

        ChainNode node = Assert.Single(report.Chain.Nodes);
        Assert.Equal(ping.Guid, node.Id);
        Assert.Equal("Ping", node.MatterType);
        Assert.Equal("Conjuring of Ping", node.ShapedBy?.Title);
        Assert.Equal(TimeSpan.Zero, node.ShapedAt?.Offset);

        Breadcrumb crumb = Assert.Single(report.Breadcrumbs);
        Assert.Equal("Ping", crumb.MatterType);
        Assert.Equal(TimeSpan.Zero, report.OccurredAt.Offset);
    }

    [Fact]
    public void Failing_weave_is_reported_too()
    {
        var spring = new Spring();
        var sink = new FakeSink();
        using var reporting = spring.EnableCrashReporting(Options(sink));
        IRzeka river = spring.Create("test", ImmediateScheduler.Instance);

        using var weave = river.Weave<Ping>(
            "ui",
            p => p.Subscribe(_ => throw new InvalidOperationException("weave boom"))
        );
        var ping = new Ping();
        river.Pluck("game", ping);

        CrashReport report = sink.Next();
        Assert.Equal("Weaving", report.Failure.Spell.School);
        Assert.Equal(new[] { ping.Guid }, report.Triggers);
    }

    [Fact]
    public void Owner_labels_are_only_sent_when_enabled()
    {
        foreach (bool enabled in new[] { false, true })
        {
            var spring = new Spring();
            var sink = new FakeSink();
            using var reporting = spring.EnableCrashReporting(Options(sink, labels: enabled));
            IRzeka river = spring.Create(
                "test",
                ImmediateScheduler.Instance,
                describeOwner: who => who.ToString()
            );
            var pings = new Subject<Ping>();
            using var strand = river.Strand(new Secretive(), pings);

            pings.OnError(new InvalidOperationException("boom"));

            FailedSpell spell = sink.Next().Failure.Spell;
            Assert.Equal(nameof(Secretive), spell.OwnerType);
            Assert.Equal(enabled ? "dear diary" : null, spell.OwnerLabel);
        }
    }

    [Fact]
    public void Stops_after_the_session_limit()
    {
        var spring = new Spring();
        var sink = new FakeSink();
        using var reporting = spring.EnableCrashReporting(Options(sink, max: 2));
        IRzeka river = spring.Create("test", ImmediateScheduler.Instance);

        for (int i = 0; i < 5; i++)
        {
            var pings = new Subject<Ping>();
            using var strand = river.Strand("game", pings);
            pings.OnError(new InvalidOperationException($"boom {i}"));
        }

        sink.Next();
        sink.Next();
        Assert.True(sink.NothingMoreWithin(TimeSpan.FromMilliseconds(200)));
    }

    [Fact]
    public void A_failing_sink_never_reaches_the_river()
    {
        var spring = new Spring();
        var sink = new FailingSink();
        using var reporting = spring.EnableCrashReporting(Options(sink));
        IRzeka river = spring.Create("test", ImmediateScheduler.Instance);
        var received = new List<Ping>();
        using var weave = river.Weave<Ping>("ui", p => p.Subscribe(received.Add));

        var failing = new Subject<Ping>();
        using var broken = river.Strand("broken", failing);
        failing.OnError(new InvalidOperationException("boom"));
        SpinWait.SpinUntil(() => Volatile.Read(ref sink.Attempts) > 0, TimeSpan.FromSeconds(5));

        river.Pluck("game", new Ping());

        Assert.Equal(1, sink.Attempts);
        Assert.Single(received);
    }

    [Fact]
    public void Requires_an_endpoint_or_a_sink()
    {
        var spring = new Spring();

        Assert.Throws<ArgumentException>(() =>
            spring.EnableCrashReporting(new CrashReportingOptions { AppName = "a", AppVersion = "1" })
        );
    }
}
