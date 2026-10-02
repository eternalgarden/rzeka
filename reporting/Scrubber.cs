using System.Text.RegularExpressions;

namespace Rzeka.Reporting;

// Errs on the side of removing too much: a report missing a detail is fine, a report
// carrying someone's journal entry is not.
internal static partial class Scrubber
{
    public static string ScrubMessage(string message)
    {
        string scrubbed = Url().Replace(message, "<url>");
        scrubbed = Email().Replace(scrubbed, "<email>");
        scrubbed = WindowsPath().Replace(scrubbed, "<path>");
        scrubbed = UnixPath().Replace(scrubbed, "<path>");
        scrubbed = DoubleQuoted().Replace(scrubbed, "\"…\"");
        scrubbed = SingleQuoted().Replace(scrubbed, "'…'");
        scrubbed = LongNumber().Replace(scrubbed, "#");
        return scrubbed;
    }

    public static string ScrubStackTrace(string stackTrace) =>
        StackFramePath().Replace(stackTrace, " in ${file}:line ${line}");

    [GeneratedRegex(@"https?://\S+")]
    private static partial Regex Url();

    [GeneratedRegex(@"[\w.+-]+@[\w-]+\.[\w.-]+")]
    private static partial Regex Email();

    [GeneratedRegex(@"[A-Za-z]:\\[^\s'""]*")]
    private static partial Regex WindowsPath();

    [GeneratedRegex(@"(?<![\w.])~?(/[^\s/'""]+){2,}/?")]
    private static partial Regex UnixPath();

    [GeneratedRegex(@"""[^""]*""")]
    private static partial Regex DoubleQuoted();

    [GeneratedRegex(@"'[^']*'")]
    private static partial Regex SingleQuoted();

    [GeneratedRegex(@"\d{4,}")]
    private static partial Regex LongNumber();

    [GeneratedRegex(@" in (?:[^\r\n]*[/\\])?(?<file>[^/\\\r\n]+):line (?<line>\d+)")]
    private static partial Regex StackFramePath();
}
