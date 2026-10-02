using Rzeka.Reporting;

namespace Rzeka.Tests.Reporting;

public class ScrubberTests
{
    [Theory]
    [InlineData("Could not find file '/home/merry/journal/dear diary.md'", "Could not find file '…'")]
    [InlineData("Could not open /home/merry/notes/today.md", "Could not open <path>")]
    [InlineData(@"Access to C:\Users\Merry\notes.txt denied", "Access to <path> denied")]
    [InlineData("Entry \"my secret\" is locked", "Entry \"…\" is locked")]
    [InlineData("Mail to merry@example.org failed", "Mail to <email> failed")]
    [InlineData("GET https://example.org/entries?id=4711 failed", "GET <url> failed")]
    [InlineData("Entry 1234567 not found", "Entry # not found")]
    public void Removes_what_could_be_personal(string message, string expected)
    {
        Assert.Equal(expected, Scrubber.ScrubMessage(message));
    }

    [Theory]
    [InlineData("Sequence contains no elements", "Sequence contains no elements")]
    [InlineData(
        "Index was out of range. Must be non-negative and less than the size of the collection.",
        "Index was out of range. Must be non-negative and less than the size of the collection."
    )]
    // Quoted parameter names go too: accepted over-scrubbing.
    [InlineData("Value cannot be null. (Parameter 'source')", "Value cannot be null. (Parameter '…')")]
    [InlineData("Expected 3 items, got 12", "Expected 3 items, got 12")]
    public void Keeps_ordinary_framework_messages_readable(string message, string expected)
    {
        Assert.Equal(expected, Scrubber.ScrubMessage(message));
    }

    [Fact]
    public void Stack_trace_keeps_file_names_but_drops_directories()
    {
        const string trace =
            "   at Player.<>c.<_Ready>b__3_0(DamageTaken d) in /home/merry/sanctuary/Player.cs:line 41\n"
            + @"   at Combat.Hit() in C:\Users\Merry\sanctuary\Combat.cs:line 7";

        string scrubbed = Scrubber.ScrubStackTrace(trace);

        Assert.Equal(
            "   at Player.<>c.<_Ready>b__3_0(DamageTaken d) in Player.cs:line 41\n"
                + "   at Combat.Hit() in Combat.cs:line 7",
            scrubbed
        );
    }

    [Fact]
    public void Stack_trace_without_paths_is_unchanged()
    {
        const string trace = "   at System.Linq.ThrowHelper.ThrowNoElementsException()";

        Assert.Equal(trace, Scrubber.ScrubStackTrace(trace));
    }
}
