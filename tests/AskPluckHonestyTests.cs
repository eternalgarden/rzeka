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
