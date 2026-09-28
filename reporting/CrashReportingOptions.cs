namespace Rzeka.Reporting;

public sealed class CrashReportingOptions
{
    // Ingest URL including its function key (?code=...). Ignored when Sink is set.
    public Uri? Endpoint { get; init; }

    public required string AppName { get; init; }
    public required string AppVersion { get; init; }

    // Scrubbed of paths, quoted text, emails, URLs and long numbers. False omits them.
    public bool IncludeExceptionMessages { get; init; } = true;

    // Sends the river's describeOwner label for the failing spell (e.g. a Godot node name).
    public bool IncludeOwnerLabels { get; init; }

    // Reports are sent only once consent is Granted; until then they're held in memory.
    // Hosts that already know the player's choice at startup set it here, others call
    // CrashReporting.SetConsent once their preferences load.
    public ReportingConsent Consent { get; init; } = ReportingConsent.Unknown;

    // Held reports count too.
    public int MaxReportsPerSession { get; init; } = 10;

    // Replaces the HTTP sink.
    public ICrashReportSink? Sink { get; init; }
}
