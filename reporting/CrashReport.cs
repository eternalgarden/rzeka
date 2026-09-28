namespace Rzeka.Reporting;

// Mirrors contract/crash-report.v1.sample.json in the rzeka-reporting repo.
public sealed record CrashReport(
    int SchemaVersion,
    Guid ReportId,
    DateTimeOffset OccurredAt,
    AppInfo App,
    RuntimeInfo Runtime,
    FailureInfo Failure,
    IReadOnlyList<Guid> Triggers,
    ChainInfo Chain,
    IReadOnlyList<Breadcrumb> Breadcrumbs
)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record AppInfo(string Name, string Version);

public sealed record RuntimeInfo(string Rzeka, string Dotnet, string Os);

public sealed record FailureInfo(FailedSpell Spell, ExceptionInfo Exception);

public sealed record FailedSpell(string Title, string School, string OwnerType, string? OwnerLabel);

public sealed record ExceptionInfo(
    string Type,
    string? Message,
    string? StackTrace,
    ExceptionInfo? Inner
);

public sealed record ChainInfo(IReadOnlyList<ChainNode> Nodes, bool Truncated);

public sealed record ChainNode(
    Guid Id,
    string MatterType,
    DateTimeOffset? ShapedAt,
    SpellRef? ShapedBy,
    IReadOnlyList<Guid> Causes
);

public sealed record SpellRef(string Title, string School, string OwnerType);

public sealed record Breadcrumb(DateTimeOffset At, string MatterType, string SpellTitle);
