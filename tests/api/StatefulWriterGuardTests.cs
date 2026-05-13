using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka.Tests;

public class StatefulWriterGuardTests
{
    [HasState]
    sealed class Score : Matter
    {
        public int Value { get; }
        public Score(int value) => Value = value;
    }

    sealed class ScoreDelta : Matter
    {
        public int Amount { get; }
        public ScoreDelta(int amount) => Amount = amount;
    }

    sealed class Plain : Matter { }

    static SpringRiver NewRiver() => (SpringRiver)new Spring().Create("test");

    [Fact]
    public void First_writer_to_a_HasState_type_registers_and_drives_the_stream()
    {
        var river = NewRiver();
        var deltas = new Subject<ScoreDelta>();
        var observed = new List<Score>();

        using var listener = river.Weave<Score>(
            "listener",
            scores => scores.Subscribe(observed.Add)
        );
        using var input = river.Strand("input", deltas);
        using var loom = river.Loom<ScoreDelta, Score>(
            "owner",
            ds => ds.Select(d => new Score(d.Amount))
        );

        deltas.OnNext(new ScoreDelta(7));

        Assert.Single(observed);
        Assert.Equal(7, observed[0].Value);
    }

    [Fact]
    public void Second_writer_to_a_HasState_type_throws_at_registration()
    {
        var river = NewRiver();

        using var first = river.Loom<ScoreDelta, Score>(
            "first",
            deltas => deltas.Select(d => new Score(d.Amount))
        );

        Assert.Throws<InvalidOperationException>(
            () =>
                river.Loom<ScoreDelta, Score>(
                    "second",
                    deltas => deltas.Select(d => new Score(d.Amount * 2))
                )
        );
    }

    [Fact]
    public void Disposing_the_first_writer_lets_the_replacement_become_the_active_reducer()
    {
        var river = NewRiver();
        var deltas = new Subject<ScoreDelta>();
        var observed = new List<Score>();

        using var listener = river.Weave<Score>(
            "listener",
            scores => scores.Subscribe(observed.Add)
        );
        using var input = river.Strand("input", deltas);

        var first = river.Loom<ScoreDelta, Score>(
            "first",
            ds => ds.Select(d => new Score(d.Amount))
        );
        first.Dispose();

        using var second = river.Loom<ScoreDelta, Score>(
            "second",
            ds => ds.Select(d => new Score(d.Amount * 2))
        );

        deltas.OnNext(new ScoreDelta(5));

        // 2x — proves the *second* loom is the live reducer, not the first.
        Assert.Single(observed);
        Assert.Equal(10, observed[0].Value);
    }

    [Fact]
    public void Strand_is_subject_to_the_same_single_writer_guard()
    {
        var river = NewRiver();

        using var firstStrand = river.Strand("first", new Subject<Score>());

        Assert.Throws<InvalidOperationException>(
            () => river.Strand("second", new Subject<Score>())
        );
    }

    [Fact]
    public void Pluck_seeds_a_HasState_type_and_a_subsequent_Loom_takes_ownership()
    {
        var river = NewRiver();
        var deltas = new Subject<ScoreDelta>();
        var observed = new List<Score>();

        // Seed before any long-lived writer — Pluck registers, fires, disposes synchronously.
        river.Pluck("seeder", new Score(0));

        using var listener = river.Weave<Score>(
            "listener",
            scores => scores.Subscribe(observed.Add)
        );
        using var input = river.Strand("input", deltas);
        using var evolver = river.Loom<ScoreDelta, Score>(
            "evolver",
            ds => ds.Select(d => new Score(d.Amount))
        );

        deltas.OnNext(new ScoreDelta(42));

        // ReplaySubject(1) on Score replays the seed to the late listener,
        // then the evolver emits the delta-driven update.
        Assert.Equal(2, observed.Count);
        Assert.Equal(0, observed[0].Value);
        Assert.Equal(42, observed[1].Value);
    }

    [Fact]
    public void Pluck_throws_when_a_long_lived_writer_already_owns_the_HasState_type()
    {
        var river = NewRiver();

        using var owner = river.Loom<ScoreDelta, Score>(
            "owner",
            deltas => deltas.Select(d => new Score(d.Amount))
        );

        Assert.Throws<InvalidOperationException>(() => river.Pluck("intruder", new Score(999)));
    }

    [Fact]
    public void Non_stateful_matter_accepts_emissions_from_multiple_concurrent_writers()
    {
        var river = NewRiver();
        var sourceA = new Subject<Plain>();
        var sourceB = new Subject<Plain>();
        var observed = new List<Plain>();

        using var listener = river.Weave<Plain>(
            "listener",
            plains => plains.Subscribe(observed.Add)
        );
        using var firstStrand = river.Strand("a", sourceA);
        using var secondStrand = river.Strand("b", sourceB);

        var fromA = new Plain();
        var fromB = new Plain();
        sourceA.OnNext(fromA);
        sourceB.OnNext(fromB);

        Assert.Equal(2, observed.Count);
        Assert.Contains(fromA, observed);
        Assert.Contains(fromB, observed);
    }
}
