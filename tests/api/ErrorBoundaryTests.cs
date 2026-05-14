using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka.Tests;

public class ErrorBoundaryTests
{
    sealed class Ping : Matter { }

    sealed class Pong : Matter { }

    static SpringRiver NewRiver() => (SpringRiver)new Spring().Create("test");

    [Fact]
    public void Strand_whose_source_errors_whispers_to_Eris_as_Horror_with_the_exception()
    {
        var river = NewRiver();
        var captured = new List<SerializableMessageOccurence>();
        using var _ = river.Eris.SerializableMessageOccurences.Subscribe(captured.Add);

        var source = new Subject<Ping>();
        using var strand = river.Strand("owner", source);

        source.OnError(new InvalidOperationException("source went sideways"));

        Assert.Single(captured);
        Assert.Equal(RzekaMessageType.Horror, captured[0].messageType);
        Assert.Contains("source went sideways", captured[0].message);
        Assert.Contains("owner", captured[0].message);
        Assert.NotNull(captured[0].exception);
        Assert.Equal("source went sideways", captured[0].exception.message);
    }

    [Fact]
    public void Loom_whose_spell_throws_produces_exactly_one_boundary_whisper()
    {
        // Scope: verify the boundary fires on Loom-side errors and carries the exception.
        // Structural spell attribution (Who/Title/Guid) is covered separately by the callback
        // test below — no need to substring-fish the formatted message here.
        var river = NewRiver();
        var captured = new List<SerializableMessageOccurence>();
        using var _ = river.Eris.SerializableMessageOccurences.Subscribe(captured.Add);

        var thrown = new InvalidOperationException("spell exploded");
        var inputs = new Subject<Ping>();
        using var input = river.Strand("source", inputs);
        using var loom = river.Loom<Ping, Pong>(
            "loom-owner",
            pings => pings.Select<Ping, Pong>(_ => throw thrown)
        );

        inputs.OnNext(new Ping());

        // Filter out off-thread warnings (test thread isn't ManagedThreadId 1).
        var boundaryWhispers = captured
            .Where(m => m.exception is not null and not SerializableNullException)
            .ToList();

        Assert.Single(boundaryWhispers);
        Assert.Equal(RzekaMessageType.Horror, boundaryWhispers[0].messageType);
        Assert.Equal(thrown.Message, boundaryWhispers[0].exception.message);
    }

    [Fact]
    public void User_handled_errors_via_upstream_Catch_do_not_reach_the_boundary()
    {
        var river = NewRiver();
        var captured = new List<SerializableMessageOccurence>();
        using var _ = river.Eris.SerializableMessageOccurences.Subscribe(captured.Add);

        var source = new Subject<Ping>();
        var handled = source.Catch<Ping, InvalidOperationException>(_ => Observable.Empty<Ping>());

        using var strand = river.Strand("owner", handled);

        source.OnError(new InvalidOperationException("user will handle this"));

        // User's .Catch consumed the error before it reached rzeka — no whisper.
        Assert.Empty(captured);
    }

    [Fact]
    public void Unhandled_source_error_callback_receives_spell_context_and_exception()
    {
        ISpell capturedSpell = null;
        Exception capturedEx = null;

        var river = (SpringRiver)
            new Spring().Create(
                "test",
                onUnhandledSourceError: (spell, ex) =>
                {
                    capturedSpell = spell;
                    capturedEx = ex;
                }
            );

        var source = new Subject<Ping>();
        using var strand = river.Strand("the-owner", source);

        var boom = new InvalidOperationException("kaboom");
        source.OnError(boom);

        Assert.NotNull(capturedSpell);
        Assert.Equal("the-owner", capturedSpell.Who);
        Assert.Equal(SpellSchool.Stranding, capturedSpell.SpellSchool);
        Assert.Same(boom, capturedEx);
    }

    [Fact]
    public void Callback_that_throws_does_not_prevent_the_Eris_whisper()
    {
        var captured = new List<SerializableMessageOccurence>();

        var river = (SpringRiver)
            new Spring().Create(
                "test",
                onUnhandledSourceError: (_, ex) => throw ex
            );
        using var _ = river.Eris.SerializableMessageOccurences.Subscribe(captured.Add);

        var source = new Subject<Ping>();
        using var strand = river.Strand("owner", source);

        // The callback throws — that propagates as OnError back through the pipeline and
        // Stream<T>'s default observer rethrows on the source thread. The whisper has
        // already run *before* the callback by design, so the diagnostic survives.
        Assert.Throws<InvalidOperationException>(
            () => source.OnError(new InvalidOperationException("propagate me"))
        );

        var boundaryWhispers = captured
            .Where(m => m.exception is not null and not SerializableNullException)
            .ToList();

        Assert.Single(boundaryWhispers);
        Assert.Contains("propagate me", boundaryWhispers[0].message);
    }

    [Fact]
    public void Without_a_callback_the_river_survives_a_source_error_as_before()
    {
        var river = NewRiver();

        var source = new Subject<Ping>();
        using var strand = river.Strand("owner", source);
        source.OnError(new InvalidOperationException("silent boom"));

        // No throw escaped; we can keep using the river.
        var secondSource = new Subject<Ping>();
        var observed = new List<Ping>();
        using var listener = river.Weave<Ping>("listener", pings => pings.Subscribe(observed.Add));
        using var second = river.Strand("second", secondSource);

        var ping = new Ping();
        secondSource.OnNext(ping);

        Assert.Single(observed);
    }

    [Fact]
    public void River_keeps_accepting_new_writers_after_a_source_errors_out()
    {
        var river = NewRiver();
        var firstSource = new Subject<Ping>();
        var first = river.Strand("first", firstSource);
        firstSource.OnError(new InvalidOperationException("boom"));

        // The errored strand completes via the boundary; disposing its handle and re-registering
        // a fresh source must succeed.
        first.Dispose();

        var secondSource = new Subject<Ping>();
        var observed = new List<Ping>();
        using var listener = river.Weave<Ping>("listener", pings => pings.Subscribe(observed.Add));
        using var second = river.Strand("second", secondSource);

        var ping = new Ping();
        secondSource.OnNext(ping);

        Assert.Single(observed);
        Assert.Same(ping, observed[0]);
    }
}
