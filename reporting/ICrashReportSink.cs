namespace Rzeka.Reporting;

// Where finished reports go. Called on a background thread, one report at a time.
public interface ICrashReportSink
{
    Task SendAsync(CrashReport report, CancellationToken cancellationToken);
}
