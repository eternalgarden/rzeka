using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Rzeka.Reporting;

namespace Rzeka.Tests.Reporting;

// The contract sample is shared with the backend repo. These tests fail when the client's
// records and the agreed JSON drift apart in either direction.
public class ContractTests
{
    static readonly string SamplePath = Path.Combine("reporting", "crash-report.v1.sample.json");

    [Fact]
    public void Sample_deserializes_without_unknown_fields()
    {
        var strict = new JsonSerializerOptions(CrashReportJson.Options)
        {
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        };

        CrashReport? report = JsonSerializer.Deserialize<CrashReport>(
            File.ReadAllText(SamplePath),
            strict
        );

        Assert.NotNull(report);
        Assert.Equal(CrashReport.CurrentSchemaVersion, report.SchemaVersion);
        Assert.Equal(2, report.Chain.Nodes.Count);
    }

    [Fact]
    public void Serialized_report_has_exactly_the_sample_fields()
    {
        JsonNode sample = JsonNode.Parse(File.ReadAllText(SamplePath))!;
        CrashReport report = JsonSerializer.Deserialize<CrashReport>(
            sample.ToJsonString(),
            CrashReportJson.Options
        )!;

        JsonNode roundTripped = JsonSerializer.SerializeToNode(report, CrashReportJson.Options)!;

        Assert.Equal(Paths(sample).Order(), Paths(roundTripped).Order());
    }

    static IEnumerable<string> Paths(JsonNode? node, string prefix = "$") =>
        node switch
        {
            JsonObject obj => obj.SelectMany(p => Paths(p.Value, $"{prefix}.{p.Key}")).Prepend(prefix),
            JsonArray arr => arr.SelectMany(item => Paths(item, $"{prefix}[]")).Prepend(prefix),
            _ => new[] { prefix },
        };
}
