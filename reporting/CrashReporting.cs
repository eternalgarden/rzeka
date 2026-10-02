using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Rzeka.Reporting;

public enum ReportingConsent
{
    // Not decided yet (e.g. preferences not loaded) reports are held in memory without sending
    Unknown,
    Granted,
    Denied,
}

public sealed class CrashReporting : IDisposable
{
    readonly CrashReportingOptions _options;
    readonly ReportSender _sender;
    readonly CompositeDisposable _teardown;

    readonly object _gate = new();
    readonly List<CrashReport> _held = [];
    ReportingConsent _consent;
    int _reportsThisSession;

    internal CrashReporting(Spring spring, CrashReportingOptions options)
    {
        if (options.Sink is null && options.Endpoint is null)
            throw new ArgumentException(
                $"Set {nameof(CrashReportingOptions.Endpoint)} or {nameof(CrashReportingOptions.Sink)}.",
                nameof(options)
            );

        _options = options;
        _consent = options.Consent;

        bool ownsSink = options.Sink is null;
        ICrashReportSink sink = options.Sink ?? new HttpCrashReportSink(options.Endpoint!);

        _sender = new ReportSender(sink);
        var currentRiverSub = new SerialDisposable();

        _teardown = new CompositeDisposable(
            spring.OnCreated.Subscribe(river => currentRiverSub.Disposable = Watch(river)),
            spring.OnDisposed.Subscribe(_ => currentRiverSub.Disposable = Disposable.Empty),
            currentRiverSub,
            _sender,
            Disposable.Create(() =>
            {
                if (ownsSink && sink is HttpCrashReportSink httpSink)
                    httpSink.Dispose();
            })
        );
    }

    public ReportingConsent Consent
    {
        get
        {
            lock (_gate)
                return _consent;
        }
    }

    // Granting consent sends potential withheld reports.
    public void SetConsent(bool granted)
    {
        lock (_gate)
        {
            _consent = granted ? ReportingConsent.Granted : ReportingConsent.Denied;
            if (granted)
                foreach (CrashReport report in _held)
                    _sender.Enqueue(report);
            else
                _held.Clear();
        }
    }

    public void Dispose() => _teardown.Dispose();

    // Only 'Shaped' matter occurrences are currently collected, recivals are filtered out.
    // Incoming miscasts are what triggers a Report.
    CompositeDisposable Watch(River river)
    {
        var memory = new RiverMemory(provenanceCapacity: 1000, breadcrumbCapacity: 20);
        return new CompositeDisposable(
            river
                .Eris.MatterOccurences.Where(o =>
                    o.MatterOccurenceCategory is MatterOccurenceCategory.Shaped
                )
                .Subscribe(occ =>
                    NeverThrow(() => memory.RecordShaped(occ.Matter, occ.Source, occ.Timestamp))
                ),
            river.Eris.Miscasts.Subscribe(miscast =>
                NeverThrow(() => Report(river, memory, miscast))
            )
        );
    }

    void Report(River river, RiverMemory memory, Miscast miscast)
    {
        lock (_gate)
        {
            if (
                _consent is ReportingConsent.Denied
                || _reportsThisSession >= _options.MaxReportsPerSession
            )
                return;
            _reportsThisSession++;

            CrashReport report = CrashReportBuilder.Build(
                miscast,
                memory,
                _options,
                river.Eris.DescribeSpellOwner,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow
            );

            if (_consent is ReportingConsent.Granted)
                _sender.Enqueue(report);
            else
                _held.Add(report);
        }
    }

    // These run inside Eris's publish calls on the river thread. A throw here would surface in
    // the spell that triggered the publish, so reporting must swallow its own failures.
    static void NeverThrow(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Trace.TraceWarning($"[rzeka] Crash reporting failed: {ex.Message}");
        }
    }
}
