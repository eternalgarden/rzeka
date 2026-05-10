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

    static SpringRiver NewRiver() => (SpringRiver)new Spring().Create("test");

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
}
