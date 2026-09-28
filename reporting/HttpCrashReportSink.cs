using System.Net.Http.Headers;
using System.Text.Json;

namespace Rzeka.Reporting;

public sealed class HttpCrashReportSink(Uri endpoint) : ICrashReportSink, IDisposable
{
    readonly HttpClient _http = new();

    public async Task SendAsync(CrashReport report, CancellationToken cancellationToken)
    {
        byte[] body = JsonSerializer.SerializeToUtf8Bytes(report, CrashReportJson.Options);
        using var content = new ByteArrayContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using HttpResponseMessage response = await _http.PostAsync(
            endpoint,
            content,
            cancellationToken
        );
        response.EnsureSuccessStatusCode();
    }

    public void Dispose() => _http.Dispose();
}
