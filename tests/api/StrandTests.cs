using System.Collections.Generic;
using System.Reactive.Subjects;

namespace Rzeka.Tests;

public class StrandTests
{
    sealed class Ink : Matter { }

    static SpringRiver NewRiver() => (SpringRiver)new Spring().Create("test", ImmediateScheduler.Instance);

    [Fact]
    public void Strand_emits_Created_and_HasMana_spell_occurences_on_construction()
    {
        // Strand is always channeling — HasMana fires immediately, unlike Loom/Weave which wait for inputs
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var _ = river.Strand(new object(), new Subject<Ink>());

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Stranding
                && o.SpellOccurenceCategory == SpellOccurenceCategory.Created
        );
        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Stranding
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
    }

    [Fact]
    public void Strand_does_not_emit_NoMana_spell_occurence()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var _ = river.Strand(new object(), new Subject<Ink>());

        Assert.DoesNotContain(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Stranding
                && o.SpellOccurenceCategory == SpellOccurenceCategory.NoMana
        );
    }

    [Fact]
    public void Strand_attributes_spell_occurences_to_who()
    {
        var river = NewRiver();
        var who = new object();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var _ = river.Strand(who, new Subject<Ink>());

        Assert.All(
            spellOccurences,
            o => Assert.Same(who, o.Source.Who)
        );
    }

    [Fact]
    public void Strand_emits_Forgotten_spell_occurence_on_dispose()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        var strand = river.Strand(new object(), new Subject<Ink>());
        strand.Dispose();

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Stranding
                && o.SpellOccurenceCategory == SpellOccurenceCategory.Forgotten
        );
    }

    [Fact]
    public void Strand_routes_emitted_matter_to_Scry_subscribers()
    {
        var river = NewRiver();
        var subject = new Subject<Ink>();
        var received = new List<Ink>();

        using var _ = river.Strand("source", subject);
        river.Scry<Ink>().Subscribe(received.Add);

        var ink1 = new Ink();
        var ink2 = new Ink();
        subject.OnNext(ink1);
        subject.OnNext(ink2);

        Assert.Equal(2, received.Count);
        Assert.Contains(received, i => i.Guid == ink1.Guid);
        Assert.Contains(received, i => i.Guid == ink2.Guid);
    }

    [Fact]
    public void Strand_emits_matter_occurence_with_Shaped_category_attributed_to_who()
    {
        var river = NewRiver();
        var who = new object();
        var subject = new Subject<Ink>();
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        using var _ = river.Strand(who, subject);

        var ink = new Ink();
        subject.OnNext(ink);

        MatterOccurence emitted = Assert.Single(matterOccurences);
        Assert.Equal(ink.Guid, emitted.Matter.Guid);
        Assert.Equal(MatterOccurenceCategory.Shaped, emitted.MatterOccurenceCategory);
        Assert.Equal(SpellSchool.Stranding, emitted.Source.SpellSchool);
        Assert.Same(who, emitted.Source.Who);
    }

    [Fact]
    public void Strand_stops_routing_after_dispose()
    {
        var river = NewRiver();
        var subject = new Subject<Ink>();
        var received = new List<Ink>();
        river.Scry<Ink>().Subscribe(received.Add);

        var strand = river.Strand("source", subject);
        subject.OnNext(new Ink());
        strand.Dispose();
        subject.OnNext(new Ink());

        Assert.Single(received);
    }

    [Fact]
    public void Multiple_Scry_subscribers_each_receive_stranded_matter()
    {
        var river = NewRiver();
        var subject = new Subject<Ink>();
        var received1 = new List<Ink>();
        var received2 = new List<Ink>();

        using var _ = river.Strand("source", subject);
        river.Scry<Ink>().Subscribe(received1.Add);
        river.Scry<Ink>().Subscribe(received2.Add);

        var ink = new Ink();
        subject.OnNext(ink);

        Assert.Single(received1);
        Assert.Single(received2);
        Assert.Equal(ink.Guid, received1[0].Guid);
        Assert.Equal(ink.Guid, received2[0].Guid);
    }
}
