using System.Collections.Generic;

namespace Rzeka.Tests;

public class PluckTests
{
    sealed class Ink : Matter { }

    static SpringRiver NewRiver() => (SpringRiver)new Spring().Create("test", ImmediateScheduler.Instance);

    // ── Lifecycle ────────────────────────────────────────────────────────────

    [Fact]
    public void Pluck_emits_Created_and_Forgotten_spell_occurences()
    {
        // Pluck is one-shot — Created → Shaped → Forgotten all fire synchronously in the constructor
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        river.Pluck(new object(), new Ink());

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Plucking
                && o.SpellOccurenceCategory == SpellOccurenceCategory.Created
        );
        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Plucking
                && o.SpellOccurenceCategory == SpellOccurenceCategory.Forgotten
        );
    }

    [Fact]
    public void Pluck_does_not_emit_HasMana_or_NoMana()
    {
        // Pluck is excluded from mana tracking — it is instantaneous, not a sustained source
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        river.Pluck(new object(), new Ink());

        Assert.DoesNotContain(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Plucking
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
        Assert.DoesNotContain(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Plucking
                && o.SpellOccurenceCategory == SpellOccurenceCategory.NoMana
        );
    }

    [Fact]
    public void Pluck_attributes_spell_occurences_to_who()
    {
        var river = NewRiver();
        var who = new object();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        river.Pluck(who, new Ink());

        Assert.All(
            spellOccurences,
            o => Assert.Same(who, o.Source.Who)
        );
    }

    // ── Emission ─────────────────────────────────────────────────────────────

    [Fact]
    public void Pluck_emits_matter_occurence_with_Shaped_category_attributed_to_who()
    {
        var river = NewRiver();
        var who = new object();
        var ink = new Ink();
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        river.Pluck(who, ink);

        MatterOccurence emitted = Assert.Single(matterOccurences);
        Assert.Equal(ink.Guid, emitted.Matter.Guid);
        Assert.Equal(MatterOccurenceCategory.Shaped, emitted.MatterOccurenceCategory);
        Assert.Equal(SpellSchool.Plucking, emitted.Source.SpellSchool);
        Assert.Same(who, emitted.Source.Who);
        Assert.False(emitted.ManualCircumstances);
    }

    [Fact]
    public void Pluck_routes_matter_to_Scry_subscribers()
    {
        var river = NewRiver();
        var received = new List<Ink>();
        river.Scry<Ink>().Subscribe(received.Add);

        var ink = new Ink();
        river.Pluck(new object(), ink);

        IMatter item = Assert.Single(received);
        Assert.Equal(ink.Guid, item.Guid);
    }

    [Fact]
    public void Pluck_preserves_manual_circumstances_and_marks_manual()
    {
        var river = NewRiver();
        var ctx1 = new Ink();
        var ctx2 = new Ink();
        Ink ink = new Ink().WithCircumstances<Ink>(ctx1, ctx2);
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        river.Pluck(new object(), ink);

        MatterOccurence emitted = Assert.Single(matterOccurences);
        Assert.True(emitted.ManualCircumstances);
        Assert.Equal(2, emitted.Matter.Circumstances.Count);
        Assert.Contains(emitted.Matter.Circumstances, c => c.Guid == ctx1.Guid);
        Assert.Contains(emitted.Matter.Circumstances, c => c.Guid == ctx2.Guid);
    }
}
