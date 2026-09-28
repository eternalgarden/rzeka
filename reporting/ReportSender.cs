using System.Diagnostics;
using System.Threading.Channels;

namespace Rzeka.Reporting;

internal sealed class ReportSender : IDisposable
{
    public const int Capacity = 16;

    // Generous: a serverless endpoint that has been idle needs a cold start first.
    static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(15);
    static readonly TimeSpan FlushTimeout = TimeSpan.FromSeconds(2);

    readonly ICrashReportSink _sink;
    readonly Channel<CrashReport> _queue = Channel.CreateBounded<CrashReport>(
        new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
        }
    );
    readonly Task _pump;

    public ReportSender(ICrashReportSink sink)
    {
        _sink = sink;
        _pump = Task.Run(PumpAsync);
    }

    // Never blocks: when the queue is full the report is dropped.
    public void Enqueue(CrashReport report) => _queue.Writer.TryWrite(report);

    async Task PumpAsync()
    {
        await foreach (CrashReport report in _queue.Reader.ReadAllAsync())
        {
            try
            {
                using var timeout = new CancellationTokenSource(SendTimeout);
                await _sink.SendAsync(report, timeout.Token);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"[rzeka] Crash report {report.ReportId} not sent: {ex.Message}");
            }
        }
    }

    // Best effort: waits up to FlushTimeout for queued reports, e.g. when the host is about to
    // crash on purpose.
    public void Dispose()
    {
        _queue.Writer.TryComplete();
        try
        {
            _pump.Wait(FlushTimeout);
        }
        catch (AggregateException) { }
    }
}
