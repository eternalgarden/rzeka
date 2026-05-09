using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Rzeka.Tests;

public class AskPluckHonestyTests
{
    sealed class Trigger : Matter { }

    sealed class TestRequest : Request { }

    sealed class TestResponse : Response<TestRequest>
    {
        public TestResponse(TestRequest request, bool wasSuccessful)
            : base(request, wasSuccessful) { }
    }

    static SpringRiver NewRiver() => (SpringRiver)new Spring().Create("test");

    [Fact]
    public void Pluck_emits_Plucking_spell_occurence_with_who()
    {
        SpringRiver river = NewRiver();
        var who = new object();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        river.Pluck(who, new Trigger());

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Plucking
                && o.Source.Who == who
                && o.SpellOccurenceCategory == SpellOccurenceCategory.Created
        );
        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Plucking
                && o.SpellOccurenceCategory == SpellOccurenceCategory.Forgotten
        );
    }

    [Fact]
    public void Pluck_emits_matter_occurence_attributed_to_who()
    {
        SpringRiver river = NewRiver();
        var who = new object();
        var trigger = new Trigger();
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        river.Pluck(who, trigger);

        MatterOccurence emitted = Assert.Single(matterOccurences);
        Assert.Equal(trigger.Guid, emitted.Matter.Guid);
        Assert.Equal(MatterOccurenceCategory.Shaped, emitted.MatterOccurenceCategory);
        Assert.Equal(SpellSchool.Plucking, emitted.Source.SpellSchool);
        Assert.Same(who, emitted.Source.Who);
        Assert.False(emitted.ManualCircumstances);
    }

    [Fact]
    public void Pluck_preserves_pre_stamped_circumstances_and_marks_manual()
    {
        SpringRiver river = NewRiver();
        var ctx1 = new Trigger();
        var ctx2 = new Trigger();
        Trigger matter = new Trigger().WithCircumstances<Trigger>(ctx1, ctx2);
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        river.Pluck(new object(), matter);

        MatterOccurence emitted = Assert.Single(matterOccurences);
        Assert.True(emitted.ManualCircumstances);
        Assert.Equal(2, emitted.Matter.Circumstances.Count);
        Assert.Contains(emitted.Matter.Circumstances, c => c.Guid == ctx1.Guid);
        Assert.Contains(emitted.Matter.Circumstances, c => c.Guid == ctx2.Guid);
    }

    [Fact]
    public void Ask_round_trips_request_to_response()
    {
        IRzeka rzeka = NewRiver();
        using IDisposable shuttle = rzeka.Shuttle<TestRequest, TestResponse>(
            "responder",
            reqs => reqs.Select(req => (TestResponse)new TestResponse(req, true))
        );

        TestResponse? received = null;
        var req = new TestRequest();
        using IDisposable sub = rzeka
            .Ask<TestRequest, TestResponse>("asker", req)
            .Take(1)
            .Subscribe(r => received = r);

        Assert.NotNull(received);
        Assert.Equal(req.Guid, received!.Request.Guid);
    }

    [Fact]
    public void Ask_response_carries_request_as_circumstance_when_responder_does_not_stamp()
    {
        IRzeka rzeka = NewRiver();
        using IDisposable shuttle = rzeka.Shuttle<TestRequest, TestResponse>(
            "responder",
            reqs => reqs.Select(req => new TestResponse(req, true))
        );

        TestResponse? received = null;
        var req = new TestRequest();
        using IDisposable sub = rzeka
            .Ask<TestRequest, TestResponse>("asker", req)
            .Take(1)
            .Subscribe(r => received = r);

        Assert.NotNull(received);
        IMatter circumstance = Assert.Single(received!.Circumstances);
        Assert.Equal(req.Guid, circumstance.Guid);
    }

    [Fact]
    public void Ask_preserves_pre_stamped_circumstances_on_request()
    {
        SpringRiver river = NewRiver();
        using IDisposable shuttle = river.Shuttle<TestRequest, TestResponse>(
            "responder",
            reqs => reqs.Select(req => new TestResponse(req, true))
        );

        var trigger = new Trigger();
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        TestRequest req = new TestRequest().WithCircumstances<TestRequest>(trigger);
        using IDisposable sub = ((IRzeka)river)
            .Ask<TestRequest, TestResponse>("asker", req)
            .Take(1)
            .Subscribe(_ => { });

        MatterOccurence requestEmission = matterOccurences.First(o => o.Matter is TestRequest);
        Assert.True(requestEmission.ManualCircumstances);
        IMatter circumstance = Assert.Single(requestEmission.Matter.Circumstances);
        Assert.Equal(trigger.Guid, circumstance.Guid);
    }

    [Fact]
    public void Shuttle_preserves_manual_circumstances_on_response()
    {
        IRzeka rzeka = NewRiver();
        var ambient = new Trigger();

        using IDisposable shuttle = rzeka.Shuttle<TestRequest, TestResponse>(
            "responder",
            reqs =>
                reqs.Select(req =>
                    new TestResponse(req, true).WithCircumstances<TestResponse>(req, ambient)
                )
        );

        TestResponse? received = null;
        var req = new TestRequest();
        using IDisposable sub = rzeka
            .Ask<TestRequest, TestResponse>("asker", req)
            .Take(1)
            .Subscribe(r => received = r);

        Assert.NotNull(received);
        Assert.Equal(2, received!.Circumstances.Count);
        Assert.Contains(received.Circumstances, c => c.Guid == req.Guid);
        Assert.Contains(received.Circumstances, c => c.Guid == ambient.Guid);
    }

    [Fact]
    public void Full_chain_attribution_for_Ask_round_trip()
    {
        SpringRiver river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        using IDisposable shuttle = river.Shuttle<TestRequest, TestResponse>(
            "responder",
            reqs => reqs.Select(req => new TestResponse(req, true))
        );

        var req = new TestRequest();
        using IDisposable sub = ((IRzeka)river)
            .Ask<TestRequest, TestResponse>("asker", req)
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
            o => o.Matter is TestRequest && o.Source.SpellSchool == SpellSchool.Plucking
        );

        Assert.Contains(
            matterOccurences,
            o => o.Matter is TestResponse && o.Source.SpellSchool == SpellSchool.Shuttling
        );
    }
}
