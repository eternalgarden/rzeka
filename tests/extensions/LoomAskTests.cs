using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka.Tests;

public class LoomAskTests
{
    sealed class Trigger : Matter { }

    sealed class Ping : Request { }

    sealed class Pong : Response<Ping>
    {
        public Pong(Ping ping, bool wasSuccessful)
            : base(ping, wasSuccessful) { }
    }

    sealed class Signal : Matter
    {
        public Guid RequestGuid { get; }
        public bool WasSuccessful { get; }

        public Signal(Pong pong)
        {
            RequestGuid = pong.Request.Guid;
            WasSuccessful = pong.WasSuccessful;
        }
    }

    static SpringRiver NewRiver() => (SpringRiver)new Spring().Create("test", ImmediateScheduler.Instance);

    // ── Basic pipeline ────────────────────────────────────────────────────────

    [Fact]
    public void Loom_uses_Ask_to_query_and_emit_one_output_per_input()
    {
        SpringRiver river = NewRiver();
        var triggerSubject = new Subject<Trigger>();
        var received = new List<Signal>();

        using var shuttle = river.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.Select(p => new Pong(p, true))
        );
        using var strand = river.Strand("source", triggerSubject);
        using var loom = river.Loom<Trigger, Signal>(
            "handler",
            triggers => triggers.SelectMany(trigger =>
                ((IRzeka)river)
                    .Ask<Ping, Pong>("handler", new Ping())
                    .Take(1)
                    .Select(pong => new Signal(pong))
            )
        );
        river.Scry<Signal>().Subscribe(received.Add);

        triggerSubject.OnNext(new Trigger());
        triggerSubject.OnNext(new Trigger());

        Assert.Equal(2, received.Count);
    }

    // ── Concurrent inputs ─────────────────────────────────────────────────────

    [Fact]
    public void Loom_Ask_concurrent_inputs_each_receive_only_their_correlated_response()
    {
        // Two triggers fire before either response arrives. The IsRespondingTo filter
        // inside Ask is the only thing keeping each in-flight query from receiving
        // the other's response. Responses arrive out of order to make the test conclusive.
        SpringRiver river = NewRiver();
        var triggerSubject = new Subject<Trigger>();
        var completions = new Subject<(Guid pingGuid, bool success)>();
        var received = new List<Signal>();

        using var shuttle = river.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.SelectMany(p =>
                completions
                    .Where(c => c.pingGuid == p.Guid)
                    .Take(1)
                    .Select(c => new Pong(p, c.success))
            )
        );

        var firedPingGuids = new List<Guid>();
        river.Eris.MatterOccurences.Subscribe(o =>
        {
            if (o.Matter is Ping && o.Source.SpellSchool == SpellSchool.Plucking)
                firedPingGuids.Add(o.Matter.Guid);
        });

        using var strand = river.Strand("source", triggerSubject);
        using var loom = river.Loom<Trigger, Signal>(
            "handler",
            triggers => triggers.SelectMany(trigger =>
                ((IRzeka)river)
                    .Ask<Ping, Pong>("handler", new Ping())
                    .Take(1)
                    .Select(pong => new Signal(pong))
            )
        );
        river.Scry<Signal>().Subscribe(received.Add);

        triggerSubject.OnNext(new Trigger()); // fires Ask → Ping1 plucked
        triggerSubject.OnNext(new Trigger()); // fires Ask → Ping2 plucked

        Assert.Equal(2, firedPingGuids.Count);
        Assert.Empty(received);

        completions.OnNext((firedPingGuids[1], true));  // Ping2 resolves first
        completions.OnNext((firedPingGuids[0], false)); // Ping1 resolves second

        Assert.Equal(2, received.Count);
        Assert.Equal(firedPingGuids[1], received[0].RequestGuid);
        Assert.True(received[0].WasSuccessful);
        Assert.Equal(firedPingGuids[0], received[1].RequestGuid);
        Assert.False(received[1].WasSuccessful);
    }

    // ── Cleanup ───────────────────────────────────────────────────────────────

    [Fact]
    public void Loom_disposal_cancels_in_flight_Ask_subscriptions()
    {
        // Disposing the Loom must dispose the SelectMany's inner subscriptions,
        // which tears down the in-flight Ask's Weave. A response arriving after
        // disposal must not produce a Signal.
        SpringRiver river = NewRiver();
        var triggerSubject = new Subject<Trigger>();
        var completions = new Subject<Guid>();
        var received = new List<Signal>();

        using var shuttle = river.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.SelectMany(p =>
                completions
                    .Where(g => g == p.Guid)
                    .Take(1)
                    .Select(_ => new Pong(p, true))
            )
        );

        var firedPingGuids = new List<Guid>();
        river.Eris.MatterOccurences.Subscribe(o =>
        {
            if (o.Matter is Ping && o.Source.SpellSchool == SpellSchool.Plucking)
                firedPingGuids.Add(o.Matter.Guid);
        });

        using var strand = river.Strand("source", triggerSubject);
        var loom = river.Loom<Trigger, Signal>(
            "handler",
            triggers => triggers.SelectMany(trigger =>
                ((IRzeka)river)
                    .Ask<Ping, Pong>("handler", new Ping())
                    .Take(1)
                    .Select(pong => new Signal(pong))
            )
        );
        river.Scry<Signal>().Subscribe(received.Add);

        triggerSubject.OnNext(new Trigger()); // Ask fired, Ping pending
        Assert.Single(firedPingGuids);
        Assert.Empty(received);

        loom.Dispose();

        completions.OnNext(firedPingGuids[0]); // response arrives after Loom is gone

        Assert.Empty(received);
    }
}
