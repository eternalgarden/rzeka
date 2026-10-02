using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka.Tests;

public class MiscastTests
{
    sealed class Ping : Matter { }

    sealed class Pong : Matter { }

    sealed class Out : Matter { }

    sealed class Question : Request { }

    sealed class Answer : Response<Question>
    {
        public Answer(Question q)
            : base(q, true) { }
    }

    sealed class Secretive
    {
        public override string ToString() => "dear diary";
    }

    static River NewRiver() =>
        (River)new Spring().Create("test", ImmediateScheduler.Instance);

    static (List<Miscast> miscasts, List<MessageOccurence> horrors) Record(River river)
    {
        var miscasts = new List<Miscast>();
        var horrors = new List<MessageOccurence>();
        river.Eris.Miscasts.Subscribe(miscasts.Add);
        river.Eris.SerializableMessageOccurences
            .Where(m => m.message.StartsWith("Unhandled error"))
            .Subscribe(m =>
                horrors.Add(
                    new MessageOccurence { Circumstances = m.circumstances, Message = m.message }
                )
            );
        return (miscasts, horrors);
    }

    // ── Conjuring boundary: triggers ─────────────────────────────────────────

    [Fact]
    public void Loom_failure_carries_the_input_it_was_handling()
    {
        var river = NewRiver();
        var (miscasts, horrors) = Record(river);
        var pings = new Subject<Ping>();
        using var strand = river.Strand("source", pings);
        using var loom = river.Loom<Ping, Out>(
            "loom",
            p => p.Select<Ping, Out>(_ => throw new InvalidOperationException("boom"))
        );

        var ping = new Ping();
        pings.OnNext(ping);

        Miscast miscast = Assert.Single(miscasts);
        Assert.Equal(ping, Assert.Single(miscast.Trigger));
        Assert.Equal(SpellSchool.Looming, miscast.Spell.SpellSchool);
        Assert.Equal(new[] { ping.Guid }, Assert.Single(horrors).Circumstances);
    }

    [Fact]
    public void Loom2_failure_carries_both_latest_inputs()
    {
        var river = NewRiver();
        var (miscasts, _) = Record(river);
        var pings = new Subject<Ping>();
        var pongs = new Subject<Pong>();
        using var s1 = river.Strand("a", pings);
        using var s2 = river.Strand("b", pongs);
        using var loom = river.Loom<Ping, Pong, Out>(
            "loom",
            (p, q) =>
                p.CombineLatest(q, (_, _) => 0)
                    .Select<int, Out>(_ => throw new InvalidOperationException("boom"))
        );

        var ping = new Ping();
        var pong = new Pong();
        pings.OnNext(ping);
        pongs.OnNext(pong);

        Assert.Equal(new IMatter[] { ping, pong }, Assert.Single(miscasts).Trigger);
    }

    [Fact]
    public void Shuttle_failure_carries_the_request()
    {
        var river = NewRiver();
        var (miscasts, _) = Record(river);
        using var shuttle = river.Shuttle<Question, Answer>(
            "shuttle",
            qs => qs.Select<Question, Answer>(_ => throw new InvalidOperationException("boom"))
        );

        var question = new Question();
        river.Pluck("asker", question);

        Assert.Equal(question, Assert.Single(Assert.Single(miscasts).Trigger));
    }

    [Fact]
    public void Strand_failure_has_no_trigger()
    {
        var river = NewRiver();
        var (miscasts, horrors) = Record(river);
        var pings = new Subject<Ping>();
        using var strand = river.Strand("source", pings);

        pings.OnError(new InvalidOperationException("boom"));

        Assert.Empty(Assert.Single(miscasts).Trigger);
        Assert.Empty(Assert.Single(horrors).Circumstances);
    }

    [Fact]
    public void Boundary_message_describes_the_owner_by_type_not_by_ToString()
    {
        var river = NewRiver();
        var (_, horrors) = Record(river);
        var pings = new Subject<Ping>();
        using var strand = river.Strand(new Secretive(), pings);

        pings.OnError(new InvalidOperationException("boom"));

        string message = Assert.Single(horrors).Message;
        Assert.Contains(nameof(Secretive), message);
        Assert.DoesNotContain("dear diary", message);
    }

    // ── Weave whisper-and-contain ────────────────────────────────────────────

    [Fact]
    public void Weave_throw_is_contained_and_whispered_with_the_matter()
    {
        var river = NewRiver();
        var (miscasts, horrors) = Record(river);
        var pings = new Subject<Ping>();
        var thrown = new InvalidOperationException("weave boom");
        using var weave = river.Weave<Ping>("weaver", p => p.Subscribe(_ => throw thrown));
        using var strand = river.Strand("source", pings);

        var ping = new Ping();
        pings.OnNext(ping);

        Miscast miscast = Assert.Single(miscasts);
        Assert.Same(thrown, miscast.Exception);
        Assert.Equal(SpellSchool.Weaving, miscast.Spell.SpellSchool);
        Assert.Equal(ping, Assert.Single(miscast.Trigger));
        Assert.Equal(new[] { ping.Guid }, Assert.Single(horrors).Circumstances);
    }

    [Fact]
    public void Weave_throw_no_longer_silences_the_stream_for_other_subscribers()
    {
        // Regression: the throw used to fault Stream<T>'s scheduled feeder, after which no
        // subscriber of that matter type ever received anything again.
        var river = NewRiver();
        int a = 0,
            b = 0;
        using var weaveA = river.Weave<Ping>(
            "A",
            p =>
                p.Subscribe(_ =>
                {
                    if (++a == 1)
                        throw new InvalidOperationException("first one only");
                })
        );
        using var weaveB = river.Weave<Ping>("B", p => p.Subscribe(_ => b++));

        river.Pluck("plucker", new Ping());
        river.Pluck("plucker", new Ping());
        river.Pluck("plucker", new Ping());

        // Rx unsubscribes an observer that throws, so the failing Weave ends (like a failing
        // source completes). Everyone else keeps receiving.
        Assert.Equal(1, a);
        Assert.Equal(3, b);
    }

    [Fact]
    public void Throw_from_an_operator_inside_the_weave_reports_the_original_exception()
    {
        var river = NewRiver();
        var (miscasts, _) = Record(river);
        var thrown = new InvalidOperationException("select boom");
        using var weave = river.Weave<Ping>(
            "weaver",
            p => p.Select<Ping, int>(_ => throw thrown).Subscribe(_ => { })
        );

        river.Pluck("plucker", new Ping());

        Assert.Same(thrown, Assert.Single(miscasts).Exception);
    }

    [Fact]
    public void Throw_crossing_several_weaves_is_whispered_once_by_the_weave_where_it_happened()
    {
        var river = NewRiver();
        var (miscasts, horrors) = Record(river);
        using var inner = river.Weave<Pong>(
            "inner",
            p => p.Subscribe(_ => throw new InvalidOperationException("inner boom"))
        );
        using var outer = river.Weave<Ping>(
            "outer",
            p => p.Subscribe(_ => river.Pluck("outer", new Pong()))
        );

        river.Pluck("plucker", new Ping());

        Miscast miscast = Assert.Single(miscasts);
        Assert.IsType<Pong>(Assert.Single(miscast.Trigger));
        Assert.Single(horrors);
    }

    [Fact]
    public void Weave_throw_invokes_the_error_callback()
    {
        var failed = new List<ISpell>();
        var river = (River)
            new Spring().Create(
                "test",
                ImmediateScheduler.Instance,
                onUnhandledSourceError: (spell, _) => failed.Add(spell)
            );
        using var weave = river.Weave<Ping>(
            "weaver",
            p => p.Subscribe(_ => throw new InvalidOperationException("weave boom"))
        );

        river.Pluck("plucker", new Ping());

        Assert.Equal(SpellSchool.Weaving, Assert.Single(failed).SpellSchool);
    }

    [Fact]
    public void Crash_on_error_callback_escapes_every_weave_and_reaches_the_caller()
    {
        var river = (River)
            new Spring().Create(
                "test",
                ImmediateScheduler.Instance,
                onUnhandledSourceError: (_, ex) => throw new ApplicationException("crash", ex)
            );
        using var inner = river.Weave<Pong>(
            "inner",
            p => p.Subscribe(_ => throw new InvalidOperationException("inner boom"))
        );
        using var outer = river.Weave<Ping>(
            "outer",
            p => p.Subscribe(_ => river.Pluck("outer", new Pong()))
        );

        var crash = Assert.Throws<ApplicationException>(() => river.Pluck("plucker", new Ping()));
        Assert.IsType<InvalidOperationException>(crash.InnerException);
    }
}
