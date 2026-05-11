using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka.Tests;

public class AskTests
{
    sealed class Ping : Request { }

    sealed class Pong : Response<Ping>
    {
        public Pong(Ping ping, bool wasSuccessful)
            : base(ping, wasSuccessful) { }
    }

    sealed class Trigger : Matter { }

    static SpringRiver NewRiver() => (SpringRiver)new Spring().Create("test");

    // ── Core behavioral ───────────────────────────────────────────────────────

    [Fact]
    public void Ask_delivers_response_correlated_to_request()
    {
        IRzeka rzeka = NewRiver();
        using var shuttle = rzeka.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.Select(p => new Pong(p, true))
        );

        Pong? received = null;
        var req = new Ping();
        using var sub = rzeka
            .Ask<Ping, Pong>("asker", req)
            .Take(1)
            .Subscribe(r => received = r);

        Assert.NotNull(received);
        Assert.Equal(req.Guid, received!.Request.Guid);
    }

    [Fact]
    public void Ask_stamps_request_as_circumstance_on_response_when_responder_does_not()
    {
        IRzeka rzeka = NewRiver();

        using var shuttle = rzeka.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.Select(p => new Pong(p, true))
        );

        Pong? received = null;
        var req = new Ping();
        using var sub = rzeka
            .Ask<Ping, Pong>("asker", req)
            .Take(1)
            .Subscribe(r => received = r);

        Assert.NotNull(received);
        IMatter circumstance = Assert.Single(received!.Circumstances);
        Assert.Equal(req.Guid, circumstance.Guid);
    }

    [Fact]
    public void Ask_does_not_strip_manual_circumstances_from_response()
    {
        IRzeka rzeka = NewRiver();
        var ambient = new Trigger();

        using var shuttle = rzeka.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.Select(p => new Pong(p, true).WithCircumstances<Pong>(p, ambient))
        );

        Pong? received = null;
        var req = new Ping();
        using var sub = rzeka
            .Ask<Ping, Pong>("asker", req)
            .Take(1)
            .Subscribe(r => received = r);

        Assert.NotNull(received);
        Assert.Equal(2, received!.Circumstances.Count);
        Assert.Contains(received.Circumstances, c => c.Guid == req.Guid);
        Assert.Contains(received.Circumstances, c => c.Guid == ambient.Guid);
    }

    // ── Cold observable ───────────────────────────────────────────────────────

    [Fact]
    public void Ask_does_not_pluck_request_before_subscription()
    {
        SpringRiver river = NewRiver();
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        using var shuttle = river.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.Select(p => new Pong(p, true))
        );

        var req = new Ping();
        IObservable<Pong> ask = ((IRzeka)river).Ask<Ping, Pong>("asker", req);

        Assert.DoesNotContain(matterOccurences, o => o.Matter is Ping);

        using var _ = ask
            .Take(1)
            .Subscribe(_ => { });

        Assert.Contains(matterOccurences, o => o.Matter is Ping && o.Matter.Guid == req.Guid);
    }

    // ── Disposal ──────────────────────────────────────────────────────────────

    [Fact]
    public void Ask_disposal_emits_Forgotten_for_inner_Weave()
    {
        SpringRiver river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var shuttle = river.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.Select(p => new Pong(p, true))
        );

        var sub = ((IRzeka)river)
            .Ask<Ping, Pong>("asker", new Ping())
            .Subscribe(_ => { });

        sub.Dispose();

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Weaving
                && o.SpellOccurenceCategory == SpellOccurenceCategory.Forgotten
                && Equals(o.Source.Who, "asker")
        );
    }

    [Fact]
    public void Ask_does_not_deliver_responses_that_arrive_after_disposal()
    {
        // The Weave is torn down on disposal; a response completing later must not reach the observer.
        SpringRiver river = NewRiver();
        var received = new List<Pong>();
        var completions = new Subject<Guid>();

        var req = new Ping();

        using var shuttle = river.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.SelectMany(p => completions
                .Where(g => g == p.Guid)
                .Take(1)
                .Select(_ => new Pong(p, true))
            )
        );

        var sub = ((IRzeka)river)
            .Ask<Ping, Pong>("asker", req)
            .Subscribe(received.Add);
        sub.Dispose();

        completions.OnNext(req.Guid); // response arrives after disposal

        Assert.Empty(received);
    }

    // ── Isolation ─────────────────────────────────────────────────────────────

    [Fact]
    public void Ask_routes_responses_only_to_their_matching_request_when_concurrent()
    {
        // Both Weaves are alive when both responses arrive, so the IsRespondingTo filter
        // is the only thing separating them.
        SpringRiver river = NewRiver();
        var received1 = new List<Pong>();
        var received2 = new List<Pong>();
        var completions = new Subject<(Guid guid, bool success)>();

        using var shuttle = river.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.SelectMany(p =>
                completions
                    .Where(c => c.guid == p.Guid)
                    .Take(1)
                    .Select(c => new Pong(p, c.success))
            )
        );

        var req1 = new Ping();
        var req2 = new Ping();

        using var sub1 = ((IRzeka)river)
            .Ask<Ping, Pong>("asker1", req1)
            .Subscribe(received1.Add);
        using var sub2 = ((IRzeka)river)
            .Ask<Ping, Pong>("asker2", req2)
            .Subscribe(received2.Add);

        completions.OnNext((req2.Guid, true));  // req2 resolves first
        completions.OnNext((req1.Guid, false)); // req1 resolves second

        Assert.Single(received1);
        Assert.Equal(req1.Guid, received1[0].Request.Guid);
        Assert.False(received1[0].WasSuccessful);

        Assert.Single(received2);
        Assert.Equal(req2.Guid, received2[0].Request.Guid);
        Assert.True(received2[0].WasSuccessful);
    }

    // ── Streaming variant ─────────────────────────────────────────────────────

    [Fact]
    public void Ask_without_Take_receives_multiple_responses_from_streaming_shuttle()
    {
        IRzeka rzeka = NewRiver();
        var received = new List<Pong>();
        var triggers = new Subject<bool>();

        var req = new Ping();

        using var shuttle = rzeka.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.SelectMany(p => triggers.Select(_ => new Pong(p, true)))
        );

        using var sub = rzeka
            .Ask<Ping, Pong>("asker", req)
            .Subscribe(received.Add);

        triggers.OnNext(true);
        triggers.OnNext(true);
        triggers.OnNext(true);

        Assert.Equal(3, received.Count);
        Assert.All(received, r => Assert.Equal(req.Guid, r.Request.Guid));
    }
}
