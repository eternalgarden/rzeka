using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Rzeka.Tests;

// These tests verify that Eris reports the truth about Ask round-trips:
// correct ManualCircumstances flags, correct SpellSchool stamps on each
// MatterOccurence, and correct Who attribution across operation boundaries.

public class AskAttributionTests
{
    sealed class Ping : Request { }

    sealed class Pong : Response<Ping>
    {
        public Pong(Ping ping, bool wasSuccessful)
            : base(ping, wasSuccessful) { }
    }

    sealed class Signal : Matter { }

    sealed class Trigger : Matter { }

    static SpringRiver NewRiver() => (SpringRiver)new Spring().Create("test", ImmediateScheduler.Instance);

    // ── Circumstance flags ────────────────────────────────────────────────────

    [Fact]
    public void Ask_request_emission_is_flagged_with_manual_circumstances_when_pre_stamped()
    {
        SpringRiver river = NewRiver();
        using var shuttle = river.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.Select(p => new Pong(p, true))
        );

        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        var trigger = new Trigger();
        Ping req = new Ping().WithCircumstances<Ping>(trigger);
        using var sub = ((IRzeka)river)
            .Ask<Ping, Pong>("asker", req)
            .Take(1)
            .Subscribe(_ => { });

        MatterOccurence emission = matterOccurences.First(o => o.Matter is Ping);
        Assert.True(emission.ManualCircumstances);
        IMatter circumstance = Assert.Single(emission.Matter.Circumstances);
        Assert.Equal(trigger.Guid, circumstance.Guid);
    }

    // ── Full chain ────────────────────────────────────────────────────────────

    [Fact]
    public void Ask_round_trip_chain_is_attributed_through_plucking_and_shuttling()
    {
        SpringRiver river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        using var shuttle = river.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.Select(p => new Pong(p, true))
        );

        var req = new Ping();
        using var sub = ((IRzeka)river)
            .Ask<Ping, Pong>("asker", req)
            .Take(1)
            .Subscribe(_ => { });

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Plucking
                && Equals(o.Source.Who, "asker")
                && o.SpellOccurenceCategory == SpellOccurenceCategory.Created
        );
        Assert.Contains(
            matterOccurences,
            o => o.Matter is Ping && o.Source.SpellSchool == SpellSchool.Plucking
        );
        Assert.Contains(
            matterOccurences,
            o => o.Matter is Pong && o.Source.SpellSchool == SpellSchool.Shuttling
        );
    }

    [Fact]
    public void Loom_processing_Ask_response_carries_full_chain_attribution()
    {
        // Shuttle emits via SpellSchool.Shuttling which publishes to the library,
        // so a Loom subscribed to Pong receives the response and continues the chain.
        SpringRiver river = NewRiver();
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        using var shuttle = river.Shuttle<Ping, Pong>(
            "responder",
            pings => pings.Select(p => new Pong(p, true))
        );
        using var loom = river.Loom<Pong, Signal>(
            "transformer",
            pongs => pongs.Select(_ => new Signal())
        );

        var req = new Ping();
        using var sub = ((IRzeka)river)
            .Ask<Ping, Pong>("asker", req)
            .Take(1)
            .Subscribe(_ => { });

        Assert.Contains(
            matterOccurences,
            o => o.Matter is Ping && o.Source.SpellSchool == SpellSchool.Plucking
        );
        Assert.Contains(
            matterOccurences,
            o => o.Matter is Pong && o.Source.SpellSchool == SpellSchool.Shuttling
        );
        Assert.Contains(
            matterOccurences,
            o =>
                o.Matter is Signal
                && o.Source.SpellSchool == SpellSchool.Looming
                && Equals(o.Source.Who, "transformer")
        );
    }
}
