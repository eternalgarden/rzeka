using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka.Tests;

public class ShuttleTests
{
    sealed class WorkOrder : Request { }

    sealed class Receipt : Response<WorkOrder>
    {
        public Receipt(WorkOrder order, bool wasSuccessful) : base(order, wasSuccessful) { }
    }

    static SpringRiver NewRiver() => (SpringRiver)new Spring().Create("test", ImmediateScheduler.Instance);

    // ── Lifecycle ────────────────────────────────────────────────────────────

    [Fact]
    public void Shuttle_emits_Created_and_NoMana_when_no_request_stream_exists()
    {
        // Shuttle extends LoomingSpell — it starts dormant with no sustained request source
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var _ = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.Select(o => new Receipt(o, true))
        );

        var shuttle = spellOccurences
            .Where(o => o.Source.SpellSchool == SpellSchool.Shuttling)
            .ToList();
        Assert.Contains(shuttle, o => o.SpellOccurenceCategory == SpellOccurenceCategory.Created);
        Assert.Contains(shuttle, o => o.SpellOccurenceCategory == SpellOccurenceCategory.NoMana);
        Assert.DoesNotContain(shuttle, o => o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana);
    }

    [Fact]
    public void Shuttle_emits_HasMana_when_a_request_strand_is_registered()
    {
        // A Strand of the request type is a sustained source — Pluck is excluded from mana tracking
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.Select(o => new Receipt(o, true))
        );
        using var strand = river.Strand("dispatcher", new Subject<WorkOrder>());

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Shuttling
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
    }

    [Fact]
    public void Shuttle_emits_Forgotten_on_dispose()
    {
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.Select(o => new Receipt(o, true))
        );
        shuttle.Dispose();

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Shuttling
                && o.SpellOccurenceCategory == SpellOccurenceCategory.Forgotten
        );
    }

    [Fact]
    public void Shuttle_attributes_spell_occurences_to_who()
    {
        var river = NewRiver();
        var who = new object();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        using var _ = river.Shuttle<WorkOrder, Receipt>(
            who,
            orders => orders.Select(o => new Receipt(o, true))
        );

        Assert.All(
            spellOccurences.Where(o => o.Source.SpellSchool == SpellSchool.Shuttling),
            o => Assert.Same(who, o.Source.Who)
        );
    }

    // ── Request-response ─────────────────────────────────────────────────────

    [Fact]
    public void Shuttle_processes_plucked_request_and_routes_response_to_Scry()
    {
        var river = NewRiver();
        var responses = new List<Receipt>();

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.Select(o => new Receipt(o, true))
        );
        river.Scry<Receipt>().Subscribe(responses.Add);

        var order = new WorkOrder();
        river.Pluck("dispatcher", order);

        Receipt receipt = Assert.Single(responses);
        Assert.Equal(order.Guid, receipt.Request.Guid);
    }

    [Fact]
    public void Shuttle_WasSuccessful_flag_flows_through_to_response()
    {
        var river = NewRiver();
        var responses = new List<Receipt>();

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.Select(o => new Receipt(o, wasSuccessful: false))
        );
        river.Scry<Receipt>().Subscribe(responses.Add);

        river.Pluck("dispatcher", new WorkOrder());

        Assert.False(Assert.Single(responses).WasSuccessful);
    }

    [Fact]
    public void Shuttle_response_carries_request_as_circumstance_automatically()
    {
        var river = NewRiver();
        var responses = new List<Receipt>();

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.Select(o => new Receipt(o, true))
        );
        river.Scry<Receipt>().Subscribe(responses.Add);

        var order = new WorkOrder();
        river.Pluck("dispatcher", order);

        Receipt receipt = Assert.Single(responses);
        IMatter circumstance = Assert.Single(receipt.Circumstances);
        Assert.Equal(order.Guid, circumstance.Guid);
    }

    [Fact]
    public void Shuttle_preserves_manual_circumstances_on_response()
    {
        var river = NewRiver();
        var ambient = new WorkOrder();
        var responses = new List<Receipt>();

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.Select(o => new Receipt(o, true).WithCircumstances<Receipt>(o, ambient))
        );
        river.Scry<Receipt>().Subscribe(responses.Add);

        var order = new WorkOrder();
        river.Pluck("dispatcher", order);

        Receipt receipt = Assert.Single(responses);
        Assert.Equal(2, receipt.Circumstances.Count);
        Assert.Contains(receipt.Circumstances, c => c.Guid == order.Guid);
        Assert.Contains(receipt.Circumstances, c => c.Guid == ambient.Guid);
    }

    [Fact]
    public void Shuttle_does_not_mark_manual_when_only_the_request_was_stamped()
    {
        // Stamping the request by hand is redundant - the request is attached automatically -
        // so the occurrence must not report hand-stamped causality to Eris.
        var river = NewRiver();
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.Select(o => new Receipt(o, true).WithCircumstances<Receipt>(o))
        );

        river.Pluck("dispatcher", new WorkOrder());

        MatterOccurence emitted = Assert.Single(matterOccurences.Where(o => o.Matter is Receipt));
        Assert.False(emitted.ManualCircumstances);
        Assert.Single(emitted.Matter.Circumstances);
    }

    [Fact]
    public void Shuttle_marks_manual_when_context_beyond_the_request_was_stamped()
    {
        // The counterpart: Scry'd context the responder stamped is causality rzeka could
        // not derive, so the occurrence must report it as manual.
        var river = NewRiver();
        var ambient = new WorkOrder();
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.Select(o => new Receipt(o, true).WithCircumstances<Receipt>(o, ambient))
        );

        river.Pluck("dispatcher", new WorkOrder());

        MatterOccurence emitted = Assert.Single(matterOccurences.Where(o => o.Matter is Receipt));
        Assert.True(emitted.ManualCircumstances);
        Assert.Equal(2, emitted.Matter.Circumstances.Count);
    }

    [Fact]
    public void Shuttle_whispers_a_Horror_when_the_response_carries_a_null_request()
    {
        // A null Request is always a construction bug: causality cannot be recorded, and
        // no Ask caller can ever be routed to the response. Silence would hide both.
        var river = NewRiver();
        var captured = new List<SerializableMessageOccurence>();
        using var _ = river.Eris.SerializableMessageOccurences.Subscribe(captured.Add);

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.Select(o => new Receipt(null, true))
        );

        river.Pluck("dispatcher", new WorkOrder());

        SerializableMessageOccurence horror = Assert.Single(
            captured.Where(m => m.message.Contains("null 'WorkOrder' request"))
        );
        Assert.Equal(RzekaMessageType.Horror, horror.messageType);
        Assert.Contains(nameof(Receipt), horror.message);
    }

    [Fact]
    public void Shuttle_whispers_the_null_request_Horror_only_once_per_spell()
    {
        // A null request is a property of how the lambda builds responses, so a broken path
        // emits it on every response. The second whisper says nothing the first did not.
        var river = NewRiver();
        var captured = new List<SerializableMessageOccurence>();
        using var _ = river.Eris.SerializableMessageOccurences.Subscribe(captured.Add);

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.Select(o => new Receipt(null, true))
        );

        var responses = new List<Receipt>();
        using var collector = river.Weave<Receipt>("collector", r => r.Subscribe(responses.Add));

        river.Pluck("dispatcher", new WorkOrder());
        river.Pluck("dispatcher", new WorkOrder());
        river.Pluck("dispatcher", new WorkOrder());

        // Guards against passing vacuously: three malformed responses really were emitted.
        Assert.Equal(3, responses.Count);
        Assert.Single(captured.Where(m => m.message.Contains("null 'WorkOrder' request")));
    }

    [Fact]
    public void Ask_is_not_faulted_by_another_responses_null_request()
    {
        // The IsRespondingTo guard: one malformed response must not throw inside the weave
        // of every other pending Ask that shares its response type.
        var river = NewRiver();
        Receipt received = null;

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.SelectMany(o => new[]
            {
                new Receipt(null, true), // malformed, passes every Ask's filter
                new Receipt(o, true),    // the real reply
            }.ToObservable())
        );

        using var ask = river
            .Ask<WorkOrder, Receipt>("caller", new WorkOrder())
            .Subscribe(r => received = r);

        Assert.NotNull(received);
        Assert.NotNull(received.Request);
    }

    // ── Async / time-spread ───────────────────────────────────────────────────

    [Fact]
    public void Shuttle_delivers_response_that_arrives_after_request()
    {
        // The core async contract: the Scry subscriber is already wired when the request is fired,
        // so a response that materialises later is still delivered correctly.
        var river = NewRiver();
        var responses = new List<Receipt>();
        var completion = new Subject<bool>();
        WorkOrder capturedOrder = null;

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.SelectMany(order =>
            {
                capturedOrder = order;
                return completion.Take(1).Select(success => new Receipt(order, success));
            })
        );
        river.Scry<Receipt>().Subscribe(responses.Add);

        river.Pluck("dispatcher", new WorkOrder());
        Assert.Empty(responses); // async work not yet done

        completion.OnNext(true); // work completes

        Receipt receipt = Assert.Single(responses);
        Assert.Equal(capturedOrder.Guid, receipt.Request.Guid);
        Assert.True(receipt.WasSuccessful);
    }

    [Fact]
    public void Shuttle_correlates_concurrent_requests_resolved_out_of_order()
    {
        // Two requests in flight at the same time; second one answered first.
        // Each response must be matched back to its own request via the Request property.
        var river = NewRiver();
        var responses = new List<Receipt>();
        var completions = new Subject<(System.Guid orderGuid, bool success)>();

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.SelectMany(order =>
                completions
                    .Where(c => c.orderGuid == order.Guid)
                    .Take(1)
                    .Select(c => new Receipt(order, c.success).WithCircumstances<Receipt>(order))
            )
        );
        river.Scry<Receipt>().Subscribe(responses.Add);

        var order1 = new WorkOrder();
        var order2 = new WorkOrder();
        river.Pluck("dispatcher", order1);
        river.Pluck("dispatcher", order2);
        Assert.Empty(responses);

        completions.OnNext((order2.Guid, true));  // order2 resolves first
        completions.OnNext((order1.Guid, false)); // order1 resolves second

        Assert.Equal(2, responses.Count);
        Assert.Equal(order2.Guid, responses[0].Request.Guid);
        Assert.Equal(order1.Guid, responses[1].Request.Guid);
        Assert.True(responses[0].WasSuccessful);
        Assert.False(responses[1].WasSuccessful);
    }

    // ── Automatic request circumstance ───────────────────────────────────────

    [Fact]
    public void Shuttle_auto_stamps_the_response_own_request_as_a_circumstance()
    {
        // The responder returns a bare response without stamping anything by hand.
        // Because Receipt : Response<WorkOrder> carries its request, the Shuttle reads
        // it off the response itself and records it as the response's circumstance.
        var river = NewRiver();
        var received = new List<Receipt>();

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.Select(o => new Receipt(o, true)) // no manual stamp
        );
        river.Scry<Receipt>().Subscribe(received.Add);

        var order = new WorkOrder();
        river.Pluck("dispatcher", order);

        Receipt receipt = Assert.Single(received);
        IMatter circumstance = Assert.Single(receipt.Circumstances);
        Assert.Equal(order.Guid, circumstance.Guid);
    }

    [Fact]
    public void Shuttle_auto_stamps_each_response_with_its_own_request_when_resolved_out_of_order()
    {
        // Two requests in flight; the second resolves first. With no manual stamping,
        // each response's auto-stamped circumstance must still be its OWN request -
        // proving the stamp reads matter.Request, not a captured "last request" that
        // would attribute both responses to whichever request arrived last.
        var river = NewRiver();
        var responses = new List<Receipt>();
        var completions = new Subject<(System.Guid orderGuid, bool success)>();

        using var shuttle = river.Shuttle<WorkOrder, Receipt>(
            "worker",
            orders => orders.SelectMany(order =>
                completions
                    .Where(c => c.orderGuid == order.Guid)
                    .Take(1)
                    .Select(c => new Receipt(order, c.success)) // no manual stamp
            )
        );
        river.Scry<Receipt>().Subscribe(responses.Add);

        var order1 = new WorkOrder();
        var order2 = new WorkOrder();
        river.Pluck("dispatcher", order1);
        river.Pluck("dispatcher", order2);

        completions.OnNext((order2.Guid, true));  // order2 resolves first
        completions.OnNext((order1.Guid, false)); // order1 resolves second

        Assert.Equal(2, responses.Count);
        Assert.Equal(order2.Guid, Assert.Single(responses[0].Circumstances).Guid);
        Assert.Equal(order1.Guid, Assert.Single(responses[1].Circumstances).Guid);
    }
}
