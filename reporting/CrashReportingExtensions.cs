namespace Rzeka.Reporting;

public static class CrashReportingExtensions
{
    // Call before spring.Create(...), like EnableDevServer: only rivers created afterwards are
    // watched.
    public static CrashReporting EnableCrashReporting(
        this Spring spring,
        CrashReportingOptions options
    ) => new(spring, options);
}
