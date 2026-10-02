using System.Linq;
using System.Reactive.Subjects;
using Rzeka.Reporting;

namespace Rzeka.Tests.Reporting;

public class RiverMemoryTests
{
    sealed class Ping : Matter { }

    static ISpell AnySpell()
    {
        var river = (River)new Spring().Create("test", ImmediateScheduler.Instance);
        ISpell spell = null!;
        river.Eris.SpellOccurences.Subscribe(o => spell ??= o.Source);
        river.Strand("game", new Subject<Ping>());
        return spell;
    }

    [Fact]
    public void Keeps_only_the_latest_breadcrumbs()
    {
        var memory = new RiverMemory(breadcrumbCapacity: 3);
        ISpell spell = AnySpell();
        var pings = Enumerable.Range(0, 5).Select(_ => new Ping()).ToArray();
        var start = DateTimeOffset.UtcNow;

        for (int i = 0; i < pings.Length; i++)
            memory.RecordShaped(pings[i], spell, start.AddSeconds(i));

        Assert.Equal(
            new[] { start.AddSeconds(2), start.AddSeconds(3), start.AddSeconds(4) },
            memory.Breadcrumbs().Select(b => b.At)
        );
    }

    [Fact]
    public void Forgets_the_oldest_provenance_beyond_capacity()
    {
        var memory = new RiverMemory(provenanceCapacity: 2);
        ISpell spell = AnySpell();
        var first = new Ping();
        var second = new Ping();
        var third = new Ping();

        memory.RecordShaped(first, spell, DateTimeOffset.UtcNow);
        memory.RecordShaped(second, spell, DateTimeOffset.UtcNow);
        memory.RecordShaped(third, spell, DateTimeOffset.UtcNow);

        Assert.Null(memory.ProvenanceOf(first.Guid));
        Assert.NotNull(memory.ProvenanceOf(second.Guid));
        Assert.NotNull(memory.ProvenanceOf(third.Guid));
    }

    [Fact]
    public void Records_times_in_utc_and_keeps_the_first_origin()
    {
        var memory = new RiverMemory();
        ISpell spell = AnySpell();
        var ping = new Ping();
        var local = new DateTimeOffset(2026, 9, 25, 14, 0, 0, TimeSpan.FromHours(2));

        memory.RecordShaped(ping, spell, local);
        memory.RecordShaped(ping, spell, local.AddMinutes(5));

        Provenance provenance = memory.ProvenanceOf(ping.Guid)!.Value;
        Assert.Equal(TimeSpan.Zero, provenance.At.Offset);
        Assert.Equal(local, provenance.At);
    }
}
