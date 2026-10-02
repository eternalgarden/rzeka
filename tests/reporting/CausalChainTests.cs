using System.Collections.Generic;
using System.Linq;
using Rzeka.Reporting;

namespace Rzeka.Tests.Reporting;

public class CausalChainTests
{
    sealed class Step : Matter { }

    static Provenance? NoProvenance(Guid _) => null;

    [Fact]
    public void Walks_a_linear_chain_from_the_trigger_back()
    {
        var root = new Step();
        var middle = new Step().WithCircumstances(root);
        var trigger = new Step().WithCircumstances(middle);

        ChainInfo chain = CausalChain.Walk(new IMatter[] { trigger }, NoProvenance);

        Assert.Equal(new[] { trigger.Guid, middle.Guid, root.Guid }, chain.Nodes.Select(n => n.Id));
        Assert.Equal(new[] { middle.Guid }, chain.Nodes[0].Causes);
        Assert.Empty(chain.Nodes[2].Causes);
        Assert.False(chain.Truncated);
    }

    [Fact]
    public void Shared_ancestors_appear_once()
    {
        var root = new Step();
        var left = new Step().WithCircumstances(root);
        var right = new Step().WithCircumstances(root);
        var trigger = new Step().WithCircumstances(left, right);

        ChainInfo chain = CausalChain.Walk(new IMatter[] { trigger }, NoProvenance);

        Assert.Equal(4, chain.Nodes.Count);
        Assert.Single(chain.Nodes, n => n.Id == root.Guid);
        Assert.Equal(new[] { left.Guid, right.Guid }, chain.Nodes[0].Causes);
    }

    [Fact]
    public void Several_triggers_are_all_included()
    {
        var a = new Step();
        var b = new Step();

        ChainInfo chain = CausalChain.Walk(new IMatter[] { a, b }, NoProvenance);

        Assert.Equal(new[] { a.Guid, b.Guid }, chain.Nodes.Select(n => n.Id));
    }

    [Fact]
    public void Long_chains_are_capped_and_marked_truncated()
    {
        // A [HasState] reducer produces exactly this: every state caused by the previous one.
        IMatter state = new Step();
        for (int i = 0; i < 100; i++)
            state = new Step().WithCircumstances(state);

        ChainInfo chain = CausalChain.Walk(new[] { state }, NoProvenance, maxNodes: 50);

        Assert.Equal(50, chain.Nodes.Count);
        Assert.True(chain.Truncated);
    }

    [Fact]
    public void Chain_of_exactly_the_cap_is_not_truncated()
    {
        IMatter state = new Step();
        for (int i = 0; i < 2; i++)
            state = new Step().WithCircumstances(state);

        ChainInfo chain = CausalChain.Walk(new[] { state }, NoProvenance, maxNodes: 3);

        Assert.Equal(3, chain.Nodes.Count);
        Assert.False(chain.Truncated);
    }

    [Fact]
    public void Provenance_is_attached_when_known()
    {
        var known = new Step();
        var unknown = new Step();
        var shapedBy = new SpellRef("Conjuring of Step", "Stranding", "Game");
        var at = new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);
        var provenance = new Dictionary<Guid, Provenance> { [known.Guid] = new(at, shapedBy) };

        ChainInfo chain = CausalChain.Walk(
            new IMatter[] { known, unknown },
            id => provenance.TryGetValue(id, out var p) ? p : null
        );

        Assert.Equal(at, chain.Nodes[0].ShapedAt);
        Assert.Equal(shapedBy, chain.Nodes[0].ShapedBy);
        Assert.Null(chain.Nodes[1].ShapedAt);
        Assert.Null(chain.Nodes[1].ShapedBy);
    }
}
