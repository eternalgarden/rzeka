using System.Text.Json;

namespace Rzeka.Reporting;

public static class CrashReportJson
{
    // camelCase property names, matching the contract.
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web);
}
