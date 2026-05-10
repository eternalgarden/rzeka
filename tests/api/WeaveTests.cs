using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka.Tests;

public class WeaveTests
{
    sealed class Ink : Matter { }
    sealed class Brush : Matter { }
    sealed class Canvas : Matter { }

    static SpringRiver NewRiver() => (SpringRiver)new Spring().Create("test");

    // ── Lifecycle ────────────────────────────────────────────────────────────

    [Fact]
    public void Weave_emits_Created_and_NoMana_when_no_input_strand_exists()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var _ = river.Weave<Ink>("who", inks => inks.Subscribe());

        var weave = spellOccurences.Where(o => o.Source.SpellSchool == SpellSchool.Weaving).ToList();
        Assert.Contains(weave, o => o.SpellOccurenceCategory == SpellOccurenceCategory.Created);
        Assert.Contains(weave, o => o.SpellOccurenceCategory == SpellOccurenceCategory.NoMana);
        Assert.DoesNotContain(weave, o => o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana);
    }

    [Fact]
    public void Weave_emits_HasMana_when_required_strand_is_registered_after()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var weave = river.Weave<Ink>("who", inks => inks.Subscribe());
        using var strand = river.Strand("source", new Subject<Ink>());

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Weaving
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
    }

    [Fact]
    public void Weave_emits_HasMana_immediately_and_skips_NoMana_when_strand_exists_before()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var strand = river.Strand("source", new Subject<Ink>());
        using var weave = river.Weave<Ink>("who", inks => inks.Subscribe());

        var weaveOccurences = spellOccurences.Where(o => o.Source.SpellSchool == SpellSchool.Weaving).ToList();
        Assert.Contains(weaveOccurences, o => o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana);
        Assert.DoesNotContain(weaveOccurences, o => o.SpellOccurenceCategory == SpellOccurenceCategory.NoMana);
    }

    [Fact]
    public void Weave_emits_Forgotten_on_dispose()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        var weave = river.Weave<Ink>("who", inks => inks.Subscribe());
        weave.Dispose();

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Weaving
                && o.SpellOccurenceCategory == SpellOccurenceCategory.Forgotten
        );
    }

    [Fact]
    public void Weave_attributes_spell_occurences_to_who()
    {
        var river = NewRiver();
        var who = new object();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var _ = river.Weave<Ink>(who, inks => inks.Subscribe());

        Assert.All(
            spellOccurences.Where(o => o.Source.SpellSchool == SpellSchool.Weaving),
            o => Assert.Same(who, o.Source.Who)
        );
    }

    // ── Consumption ──────────────────────────────────────────────────────────

    [Fact]
    public void Weave_receives_matter_emitted_by_strand()
    {
        var river = NewRiver();
        var subject = new Subject<Ink>();
        var received = new List<Ink>();

        using var strand = river.Strand("source", subject);
        using var weave = river.Weave<Ink>("consumer", inks => inks.Subscribe(received.Add));

        var ink1 = new Ink();
        var ink2 = new Ink();
        subject.OnNext(ink1);
        subject.OnNext(ink2);

        Assert.Equal(2, received.Count);
        Assert.Contains(received, i => i.Guid == ink1.Guid);
        Assert.Contains(received, i => i.Guid == ink2.Guid);
    }

    [Fact]
    public void Weave_receives_from_strand_registered_after_weave()
    {
        // The Stream<T> subject hub means a Weave subscribed before a Strand exists
        // will still receive items once the Strand is registered later.
        var river = NewRiver();
        var subject = new Subject<Ink>();
        var received = new List<Ink>();

        using var weave = river.Weave<Ink>("consumer", inks => inks.Subscribe(received.Add));
        using var strand = river.Strand("source", subject);

        subject.OnNext(new Ink());

        Assert.Single(received);
    }

    [Fact]
    public void Weave_emits_Received_matter_occurence_per_emission()
    {
        var river = NewRiver();
        var subject = new Subject<Ink>();
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        using var strand = river.Strand("source", subject);
        using var weave = river.Weave<Ink>("consumer", inks => inks.Subscribe());

        subject.OnNext(new Ink());

        Assert.Contains(
            matterOccurences,
            o =>
                o.Matter is Ink
                && o.MatterOccurenceCategory == MatterOccurenceCategory.Received
                && o.Source.SpellSchool == SpellSchool.Weaving
        );
    }

    [Fact]
    public void Weave_does_not_receive_after_dispose()
    {
        var river = NewRiver();
        var subject = new Subject<Ink>();
        var received = new List<Ink>();

        using var strand = river.Strand("source", subject);
        var weave = river.Weave<Ink>("consumer", inks => inks.Subscribe(received.Add));

        subject.OnNext(new Ink());
        weave.Dispose();
        subject.OnNext(new Ink());

        Assert.Single(received);
    }

    // ── Observer overload ─────────────────────────────────────────────────────

    [Fact]
    public void Weave_observer_overload_delivers_matter_to_observer_OnNext()
    {
        var river = NewRiver();
        var subject = new Subject<Ink>();
        var received = new List<Ink>();

        // Subject<T> implements IObserver<T> — clean way to capture calls
        var observer = new Subject<Ink>();
        observer.Subscribe(received.Add);

        using var strand = river.Strand("source", subject);
        using var weave = river.Weave<Ink>("consumer", observer);

        var ink = new Ink();
        subject.OnNext(ink);

        IMatter item = Assert.Single(received);
        Assert.Equal(ink.Guid, item.Guid);
    }

    // ── Two-input variant ─────────────────────────────────────────────────────

    [Fact]
    public void Weave2_stays_NoMana_when_only_one_of_two_inputs_is_stranded()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var weave = river.Weave<Ink, Brush>(
            "who",
            (inks, brushes) => new CompositeDisposable(inks.Subscribe(), brushes.Subscribe())
        );
        using var inkStrand = river.Strand("s1", new Subject<Ink>());

        Assert.DoesNotContain(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Weaving
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
    }

    [Fact]
    public void Weave2_emits_HasMana_when_both_inputs_are_stranded()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var weave = river.Weave<Ink, Brush>(
            "who",
            (inks, brushes) => new CompositeDisposable(inks.Subscribe(), brushes.Subscribe())
        );
        using var inkStrand = river.Strand("s1", new Subject<Ink>());
        using var brushStrand = river.Strand("s2", new Subject<Brush>());

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Weaving
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
    }

    [Fact]
    public void Weave2_emits_NoMana_when_one_input_strand_is_disposed()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var weave = river.Weave<Ink, Brush>(
            "who",
            (inks, brushes) => new CompositeDisposable(inks.Subscribe(), brushes.Subscribe())
        );
        using var inkStrand = river.Strand("s1", new Subject<Ink>());
        var brushStrand = river.Strand("s2", new Subject<Brush>());
        brushStrand.Dispose();

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Weaving
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Weaving
                && o.SpellOccurenceCategory == SpellOccurenceCategory.NoMana
        );
    }

    [Fact]
    public void Weave2_receives_from_both_input_strands()
    {
        var river = NewRiver();
        var inkSubject = new Subject<Ink>();
        var brushSubject = new Subject<Brush>();
        var inkReceived = new List<Ink>();
        var brushReceived = new List<Brush>();

        using var s1 = river.Strand("s1", inkSubject);
        using var s2 = river.Strand("s2", brushSubject);
        using var weave = river.Weave<Ink, Brush>(
            "consumer",
            (inks, brushes) => new CompositeDisposable(
                inks.Subscribe(inkReceived.Add),
                brushes.Subscribe(brushReceived.Add)
            )
        );

        inkSubject.OnNext(new Ink());
        brushSubject.OnNext(new Brush());

        Assert.Single(inkReceived);
        Assert.Single(brushReceived);
    }

    // ── Three-input variant ───────────────────────────────────────────────────

    [Fact]
    public void Weave3_stays_NoMana_when_only_two_of_three_inputs_are_stranded()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var weave = river.Weave<Ink, Brush, Canvas>(
            "who",
            (inks, brushes, canvases) =>
                new CompositeDisposable(inks.Subscribe(), brushes.Subscribe(), canvases.Subscribe())
        );
        using var s1 = river.Strand("s1", new Subject<Ink>());
        using var s2 = river.Strand("s2", new Subject<Brush>());

        Assert.DoesNotContain(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Weaving
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
    }

    [Fact]
    public void Weave3_emits_HasMana_when_all_three_inputs_are_stranded()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var weave = river.Weave<Ink, Brush, Canvas>(
            "who",
            (inks, brushes, canvases) =>
                new CompositeDisposable(inks.Subscribe(), brushes.Subscribe(), canvases.Subscribe())
        );
        using var s1 = river.Strand("s1", new Subject<Ink>());
        using var s2 = river.Strand("s2", new Subject<Brush>());
        using var s3 = river.Strand("s3", new Subject<Canvas>());

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Weaving
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
    }

    [Fact]
    public void Weave3_emits_NoMana_when_one_input_strand_is_disposed()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var weave = river.Weave<Ink, Brush, Canvas>(
            "who",
            (inks, brushes, canvases) =>
                new CompositeDisposable(inks.Subscribe(), brushes.Subscribe(), canvases.Subscribe())
        );
        using var s1 = river.Strand("s1", new Subject<Ink>());
        using var s2 = river.Strand("s2", new Subject<Brush>());
        var s3 = river.Strand("s3", new Subject<Canvas>());
        s3.Dispose();

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Weaving
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Weaving
                && o.SpellOccurenceCategory == SpellOccurenceCategory.NoMana
        );
    }

    [Fact]
    public void Weave3_receives_from_all_three_input_strands()
    {
        var river = NewRiver();
        var inkSubject = new Subject<Ink>();
        var brushSubject = new Subject<Brush>();
        var canvasSubject = new Subject<Canvas>();
        var inkReceived = new List<Ink>();
        var brushReceived = new List<Brush>();
        var canvasReceived = new List<Canvas>();

        using var s1 = river.Strand("s1", inkSubject);
        using var s2 = river.Strand("s2", brushSubject);
        using var s3 = river.Strand("s3", canvasSubject);
        using var weave = river.Weave<Ink, Brush, Canvas>(
            "consumer",
            (inks, brushes, canvases) => new CompositeDisposable(
                inks.Subscribe(inkReceived.Add),
                brushes.Subscribe(brushReceived.Add),
                canvases.Subscribe(canvasReceived.Add)
            )
        );

        inkSubject.OnNext(new Ink());
        brushSubject.OnNext(new Brush());
        canvasSubject.OnNext(new Canvas());

        Assert.Single(inkReceived);
        Assert.Single(brushReceived);
        Assert.Single(canvasReceived);
    }
}
