using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka.Tests;

public class LoomTests
{
    sealed class Ink : Matter { }
    sealed class Paint : Matter { }
    sealed class Brush : Matter { }
    sealed class Canvas : Matter { }

    static SpringRiver NewRiver() => (SpringRiver)new Spring().Create("test");

    // ── Lifecycle ────────────────────────────────────────────────────────────

    [Fact]
    public void Loom_emits_Created_and_NoMana_when_no_input_strand_exists()
    {
        // Loom starts dormant — unlike Strand, it needs its input type in the river before it can run
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var _ = river.Loom<Ink, Paint>("who", inks => inks.Select(_ => new Paint()));

        var loom = spellOccurences.Where(o => o.Source.SpellSchool == SpellSchool.Looming).ToList();
        Assert.Contains(loom, o => o.SpellOccurenceCategory == SpellOccurenceCategory.Created);
        Assert.Contains(loom, o => o.SpellOccurenceCategory == SpellOccurenceCategory.NoMana);
        Assert.DoesNotContain(loom, o => o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana);
    }

    [Fact]
    public void Loom_emits_HasMana_when_required_strand_is_registered_after()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var loom = river.Loom<Ink, Paint>("who", inks => inks.Select(_ => new Paint()));
        using var strand = river.Strand("source", new Subject<Ink>());

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Looming
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
    }

    [Fact]
    public void Loom_emits_HasMana_immediately_and_skips_NoMana_when_strand_exists_before()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var strand = river.Strand("source", new Subject<Ink>());
        using var loom = river.Loom<Ink, Paint>("who", inks => inks.Select(_ => new Paint()));

        var loomOccurences = spellOccurences.Where(o => o.Source.SpellSchool == SpellSchool.Looming).ToList();
        Assert.Contains(loomOccurences, o => o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana);
        Assert.DoesNotContain(loomOccurences, o => o.SpellOccurenceCategory == SpellOccurenceCategory.NoMana);
    }

    [Fact]
    public void Loom_emits_NoMana_when_input_strand_is_disposed()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var loom = river.Loom<Ink, Paint>("who", inks => inks.Select(_ => new Paint()));
        var strand = river.Strand("source", new Subject<Ink>());
        strand.Dispose();

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Looming
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Looming
                && o.SpellOccurenceCategory == SpellOccurenceCategory.NoMana
        );
    }

    [Fact]
    public void Loom_emits_Forgotten_on_dispose()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        var loom = river.Loom<Ink, Paint>("who", inks => inks.Select(_ => new Paint()));
        loom.Dispose();

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Looming
                && o.SpellOccurenceCategory == SpellOccurenceCategory.Forgotten
        );
    }

    [Fact]
    public void Loom_attributes_spell_occurences_to_who()
    {
        var river = NewRiver();
        var who = new object();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var _ = river.Loom<Ink, Paint>(who, inks => inks.Select(_ => new Paint()));

        Assert.All(
            spellOccurences.Where(o => o.Source.SpellSchool == SpellSchool.Looming),
            o => Assert.Same(who, o.Source.Who)
        );
    }

    // ── Routing ──────────────────────────────────────────────────────────────

    [Fact]
    public void Loom_transforms_input_and_routes_output_via_Scry()
    {
        var river = NewRiver();
        var subject = new Subject<Ink>();
        var received = new List<Paint>();

        using var strand = river.Strand("source", subject);
        using var loom = river.Loom<Ink, Paint>("transformer", inks => inks.Select(_ => new Paint()));
        river.Scry<Paint>().Subscribe(received.Add);

        subject.OnNext(new Ink());
        subject.OnNext(new Ink());

        Assert.Equal(2, received.Count);
    }

    [Fact]
    public void Loom_output_carries_input_as_circumstance_automatically()
    {
        var river = NewRiver();
        var subject = new Subject<Ink>();
        var received = new List<Paint>();

        using var strand = river.Strand("source", subject);
        using var loom = river.Loom<Ink, Paint>("transformer", inks => inks.Select(_ => new Paint()));
        river.Scry<Paint>().Subscribe(received.Add);

        var ink = new Ink();
        subject.OnNext(ink);

        Paint paint = Assert.Single(received);
        IMatter circumstance = Assert.Single(paint.Circumstances);
        Assert.Equal(ink.Guid, circumstance.Guid);
    }

    [Fact]
    public void Loom_preserves_manual_circumstances_on_output()
    {
        var river = NewRiver();
        var subject = new Subject<Ink>();
        var ambient = new Ink();
        var received = new List<Paint>();

        using var strand = river.Strand("source", subject);
        using var loom = river.Loom<Ink, Paint>(
            "transformer",
            inks => inks.Select(ink => new Paint().WithCircumstances<Paint>(ink, ambient))
        );
        river.Scry<Paint>().Subscribe(received.Add);

        var ink = new Ink();
        subject.OnNext(ink);

        Paint paint = Assert.Single(received);
        Assert.Equal(2, paint.Circumstances.Count);
        Assert.Contains(paint.Circumstances, c => c.Guid == ink.Guid);
        Assert.Contains(paint.Circumstances, c => c.Guid == ambient.Guid);
    }

    [Fact]
    public void Loom_emits_Received_and_Shaped_matter_occurences_per_emission()
    {
        var river = NewRiver();
        var subject = new Subject<Ink>();
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        using var strand = river.Strand("source", subject);
        using var loom = river.Loom<Ink, Paint>("transformer", inks => inks.Select(_ => new Paint()));

        subject.OnNext(new Ink());

        Assert.Contains(
            matterOccurences,
            o =>
                o.Matter is Ink
                && o.MatterOccurenceCategory == MatterOccurenceCategory.Received
                && o.Source.SpellSchool == SpellSchool.Looming
        );
        Assert.Contains(
            matterOccurences,
            o =>
                o.Matter is Paint
                && o.MatterOccurenceCategory == MatterOccurenceCategory.Shaped
                && o.Source.SpellSchool == SpellSchool.Looming
        );
    }

    // ── Two-input variant ─────────────────────────────────────────────────────

    [Fact]
    public void Loom2_stays_NoMana_when_only_one_of_two_inputs_is_stranded()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var loom = river.Loom<Ink, Brush, Paint>(
            "who",
            (inks, brushes) => inks.CombineLatest(brushes, (_, _) => new Paint())
        );
        using var inkStrand = river.Strand("s1", new Subject<Ink>());

        Assert.DoesNotContain(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Looming
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
    }

    [Fact]
    public void Loom2_emits_HasMana_when_both_inputs_are_stranded()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var loom = river.Loom<Ink, Brush, Paint>(
            "who",
            (inks, brushes) => inks.CombineLatest(brushes, (_, _) => new Paint())
        );
        using var inkStrand = river.Strand("s1", new Subject<Ink>());
        using var brushStrand = river.Strand("s2", new Subject<Brush>());

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Looming
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
    }

    [Fact]
    public void Loom2_emits_NoMana_when_one_of_the_input_strands_is_forgotten()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var loom = river.Loom<Ink, Brush, Paint>(
            "who",
            (inks, brushes) => inks.CombineLatest(brushes, (_, _) => new Paint())
        );
        using var inkStrand = river.Strand("s1", new Subject<Ink>());
        var brushStrand = river.Strand("s2", new Subject<Brush>());
        brushStrand.Dispose();

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Looming
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Looming
                && o.SpellOccurenceCategory == SpellOccurenceCategory.NoMana
        );
    }


    [Fact]
    public void Loom2_output_carries_both_inputs_as_circumstances_automatically()
    {
        var river = NewRiver();
        var inkSubject = new Subject<Ink>();
        var brushSubject = new Subject<Brush>();
        var received = new List<Paint>();

        using var s1 = river.Strand("s1", inkSubject);
        using var s2 = river.Strand("s2", brushSubject);
        using var loom = river.Loom<Ink, Brush, Paint>(
            "transformer",
            (inks, brushes) => inks.CombineLatest(brushes, (_, _) => new Paint())
        );
        river.Scry<Paint>().Subscribe(received.Add);

        var ink = new Ink();
        var brush = new Brush();
        inkSubject.OnNext(ink);
        brushSubject.OnNext(brush);

        Paint paint = Assert.Single(received);
        Assert.Equal(2, paint.Circumstances.Count);
        Assert.Contains(paint.Circumstances, c => c.Guid == ink.Guid);
        Assert.Contains(paint.Circumstances, c => c.Guid == brush.Guid);
    }

    // ── Three-input variant ───────────────────────────────────────────────────

    [Fact]
    public void Loom3_stays_NoMana_when_only_two_of_three_inputs_are_stranded()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var loom = river.Loom<Ink, Brush, Canvas, Paint>(
            "who",
            (inks, brushes, canvases) =>
                inks.CombineLatest(brushes, canvases, (_, _, _) => new Paint())
        );
        using var s1 = river.Strand("s1", new Subject<Ink>());
        using var s2 = river.Strand("s2", new Subject<Brush>());

        Assert.DoesNotContain(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Looming
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
    }

    [Fact]
    public void Loom3_emits_HasMana_when_all_three_inputs_are_stranded()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var loom = river.Loom<Ink, Brush, Canvas, Paint>(
            "who",
            (inks, brushes, canvases) =>
                inks.CombineLatest(brushes, canvases, (_, _, _) => new Paint())
        );
        using var s1 = river.Strand("s1", new Subject<Ink>());
        using var s2 = river.Strand("s2", new Subject<Brush>());
        using var s3 = river.Strand("s3", new Subject<Canvas>());

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Looming
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
    }

    [Fact]
    public void Loom3_emits_NoMana_when_one_of_the_input_strands_is_forgotten()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var loom = river.Loom<Ink, Brush, Paint>(
            "who",
            (inks, brushes) => inks.CombineLatest(brushes, (_, _) => new Paint())
        );

        using var s1 = river.Strand("s1", new Subject<Ink>());
        using var s2 = river.Strand("s2", new Subject<Brush>());
        using var s3 = river.Strand("s3", new Subject<Canvas>());
        s3.Dispose();

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Looming
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Looming
                && o.SpellOccurenceCategory == SpellOccurenceCategory.NoMana
        );
    }

    [Fact]
    public void Loom3_output_carries_all_three_inputs_as_circumstances_automatically()
    {
        var river = NewRiver();
        var inkSubject = new Subject<Ink>();
        var brushSubject = new Subject<Brush>();
        var canvasSubject = new Subject<Canvas>();
        var received = new List<Paint>();

        using var s1 = river.Strand("s1", inkSubject);
        using var s2 = river.Strand("s2", brushSubject);
        using var s3 = river.Strand("s3", canvasSubject);
        using var loom = river.Loom<Ink, Brush, Canvas, Paint>(
            "transformer",
            (inks, brushes, canvases) =>
                inks.CombineLatest(brushes, canvases, (_, _, _) => new Paint())
        );
        river.Scry<Paint>().Subscribe(received.Add);

        var ink = new Ink();
        var brush = new Brush();
        var canvas = new Canvas();
        inkSubject.OnNext(ink);
        brushSubject.OnNext(brush);
        canvasSubject.OnNext(canvas);

        Paint paint = Assert.Single(received);
        Assert.Equal(3, paint.Circumstances.Count);
        Assert.Contains(paint.Circumstances, c => c.Guid == ink.Guid);
        Assert.Contains(paint.Circumstances, c => c.Guid == brush.Guid);
        Assert.Contains(paint.Circumstances, c => c.Guid == canvas.Guid);
    }

    // ── Null-lastT guard ─────────────────────────────────────────────────────

    [Fact]
    public void Loom1_output_has_empty_circumstances_when_spell_fires_before_ingredient_emits()
    {
        // Spell returns Observable.Return without subscribing to the ingredient observable.
        // lastT stays null — output should have empty circumstances, not crash.
        var river = NewRiver();
        var received = new List<Paint>();
        river.Scry<Paint>().Subscribe(received.Add); // subscribe before emission fires

        using var loom = river.Loom<Ink, Paint>(
            "transformer",
            inks => Observable.Return(new Paint())
        );
        using var strand = river.Strand("source", new Subject<Ink>()); // gives Loom mana → emission fires

        Paint paint = Assert.Single(received);
        Assert.Empty(paint.Circumstances);
    }

    [Fact]
    public void Loom2_output_omits_unused_ingredient_from_circumstances()
    {
        // Second ingredient observable is declared but never wired into the chain.
        // lastT2 stays null — only lastT1 (ink) should appear as a circumstance.
        var river = NewRiver();
        var inkSubject = new Subject<Ink>();
        var received = new List<Paint>();

        using var s1 = river.Strand("s1", inkSubject);
        using var s2 = river.Strand("s2", new Subject<Brush>());
        using var loom = river.Loom<Ink, Brush, Paint>(
            "transformer",
            (inks, brushes) => inks.Select(_ => new Paint())
        );
        river.Scry<Paint>().Subscribe(received.Add);

        var ink = new Ink();
        inkSubject.OnNext(ink);

        Paint paint = Assert.Single(received);
        Assert.Equal(1, paint.Circumstances.Count);
        Assert.Equal(ink.Guid, paint.Circumstances[0].Guid);
    }

    [Fact]
    public void Loom3_output_omits_unused_ingredients_from_circumstances()
    {
        // Second and third ingredient observables are declared but never wired into the chain.
        // lastT2 and lastT3 stay null — only lastT1 (ink) should appear as a circumstance.
        var river = NewRiver();
        var inkSubject = new Subject<Ink>();
        var received = new List<Paint>();

        using var s1 = river.Strand("s1", inkSubject);
        using var s2 = river.Strand("s2", new Subject<Brush>());
        using var s3 = river.Strand("s3", new Subject<Canvas>());
        using var loom = river.Loom<Ink, Brush, Canvas, Paint>(
            "transformer",
            (inks, brushes, canvases) => inks.Select(_ => new Paint())
        );
        river.Scry<Paint>().Subscribe(received.Add);

        var ink = new Ink();
        inkSubject.OnNext(ink);

        Paint paint = Assert.Single(received);
        Assert.Equal(1, paint.Circumstances.Count);
        Assert.Equal(ink.Guid, paint.Circumstances[0].Guid);
    }
}
