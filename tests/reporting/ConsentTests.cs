using System.Reactive.Subjects;
using Rzeka.Reporting;
using static Rzeka.Tests.Reporting.CrashReportingTests;

namespace Rzeka.Tests.Reporting;

public class ConsentTests
{
    sealed class Ping : Matter { }

    static readonly TimeSpan Quiet = TimeSpan.FromMilliseconds(200);

    static (CrashReporting reporting, FakeSink sink, IRzeka river) Setup(
        ReportingConsent consent = ReportingConsent.Unknown,
        int max = 10
    )
    {
        var spring = new Spring();
        var sink = new FakeSink();
        CrashReporting reporting = spring.EnableCrashReporting(Options(sink, max, consent: consent));
        IRzeka river = spring.Create("test", ImmediateScheduler.Instance);
        return (reporting, sink, river);
    }

    static void Fail(IRzeka river)
    {
        var pings = new Subject<Ping>();
        using var strand = river.Strand("game", pings);
        pings.OnError(new InvalidOperationException("boom"));
    }

    [Fact]
    public void Consent_is_unknown_by_default()
    {
        Assert.Equal(
            ReportingConsent.Unknown,
            new CrashReportingOptions { AppName = "a", AppVersion = "1" }.Consent
        );
    }

    [Fact]
    public void Without_consent_nothing_is_sent_until_it_is_granted()
    {
        var (reporting, sink, river) = Setup();
        using var _ = reporting;

        Fail(river);
        Assert.True(sink.NothingMoreWithin(Quiet));

        reporting.SetConsent(granted: true);
        sink.Next();
    }

    [Fact]
    public void Denying_discards_held_reports_and_stops_new_ones()
    {
        var (reporting, sink, river) = Setup();
        using var _ = reporting;

        Fail(river);
        reporting.SetConsent(granted: false);
        Fail(river);

        Assert.True(sink.NothingMoreWithin(Quiet));
        Assert.Equal(ReportingConsent.Denied, reporting.Consent);
    }

    [Fact]
    public void Granting_after_denying_sends_only_new_failures()
    {
        var (reporting, sink, river) = Setup();
        using var _ = reporting;

        Fail(river);
        reporting.SetConsent(granted: false);
        reporting.SetConsent(granted: true);
        Assert.True(sink.NothingMoreWithin(Quiet));

        Fail(river);
        sink.Next();
    }

    [Fact]
    public void Granted_at_startup_sends_immediately()
    {
        var (reporting, sink, river) = Setup(ReportingConsent.Granted);
        using var _ = reporting;

        Fail(river);

        sink.Next();
    }

    [Fact]
    public void Held_reports_count_towards_the_session_limit()
    {
        var (reporting, sink, river) = Setup(max: 2);
        using var _ = reporting;

        for (int i = 0; i < 5; i++)
            Fail(river);
        reporting.SetConsent(granted: true);

        sink.Next();
        sink.Next();
        Assert.True(sink.NothingMoreWithin(Quiet));
    }
}
