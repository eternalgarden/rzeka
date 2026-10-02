namespace Rzeka.Reporting;

internal static class CrashReportBuilder
{
    const int MaxInnerExceptions = 3;

    public static CrashReport Build(
        Miscast miscast,
        RiverMemory memory,
        CrashReportingOptions options,
        Func<object, string?>? describeOwner,
        Guid reportId,
        DateTimeOffset now
    )
    {
        ISpell spell = miscast.Spell;
        string? ownerLabel = options.IncludeOwnerLabels ? describeOwner?.Invoke(spell.Who) : null;

        return new CrashReport(
            CrashReport.CurrentSchemaVersion,
            reportId,
            now.ToUniversalTime(),
            new AppInfo(options.AppName, options.AppVersion),
            RuntimeSnapshot.Current,
            new FailureInfo(
                new FailedSpell(
                    spell.Title,
                    spell.SpellSchool.ToString(),
                    spell.Who.GetType().Name,
                    ownerLabel
                ),
                DescribeException(miscast.Exception, options.IncludeExceptionMessages, depth: 0)
            ),
            [.. miscast.Trigger.Select(m => m.Guid)],
            CausalChain.Walk(miscast.Trigger, memory.ProvenanceOf),
            memory.Breadcrumbs()
        );
    }

    static ExceptionInfo DescribeException(Exception ex, bool includeMessage, int depth) =>
        new(
            ex.GetType().FullName ?? ex.GetType().Name,
            includeMessage ? Scrubber.ScrubMessage(ex.Message) : null,
            ex.StackTrace is null ? null : Scrubber.ScrubStackTrace(ex.StackTrace),
            ex.InnerException is not null && depth + 1 < MaxInnerExceptions
                ? DescribeException(ex.InnerException, includeMessage, depth + 1)
                : null
        );
}

internal static class RuntimeSnapshot
{
    // OS family only: a full OS description (kernel build, distro) would fingerprint the machine.
    public static RuntimeInfo Current { get; } =
        new(
            typeof(IMatter).Assembly.GetName().Version?.ToString(3) ?? "unknown",
            Environment.Version.ToString(),
            OperatingSystem.IsWindows() ? "Windows"
            : OperatingSystem.IsMacOS() ? "macOS"
            : OperatingSystem.IsLinux() ? "Linux"
            : OperatingSystem.IsAndroid() ? "Android"
            : OperatingSystem.IsIOS() ? "iOS"
            : "Other"
        );
}
