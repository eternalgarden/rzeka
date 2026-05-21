using System.Collections.Generic;
using System.Reactive.Subjects;

namespace Rzeka.Tests;

public class ScrollExtensionTests
{
    // ── CombineLatest (pair) ──────────────────────────────────────────────────

    [Fact]
    public void CombineLatestMatter_pair_emits_tuple_when_left_fires_after_both_have_emitted()
    {
        var left = new Subject<int>();
        var right = new Subject<string>();
        var received = new List<(int, string)>();

        using var _ = left.CombineLatestMatter(right).Subscribe(received.Add);

        right.OnNext("a");
        left.OnNext(1);
        left.OnNext(2);

        Assert.Equal(2, received.Count);
        Assert.Equal((1, "a"), received[0]);
        Assert.Equal((2, "a"), received[1]);
    }

    [Fact]
    public void CombineLatestMatter_pair_emits_tuple_when_right_fires_after_both_have_emitted()
    {
        var left = new Subject<int>();
        var right = new Subject<string>();
        var received = new List<(int, string)>();

        using var _ = left.CombineLatestMatter(right).Subscribe(received.Add);

        left.OnNext(1);
        right.OnNext("a");
        right.OnNext("b");

        Assert.Equal(2, received.Count);
        Assert.Equal((1, "a"), received[0]);
        Assert.Equal((1, "b"), received[1]);
    }

    [Fact]
    public void CombineLatestMatter_pair_does_not_emit_until_both_have_fired()
    {
        var left = new Subject<int>();
        var right = new Subject<string>();
        var received = new List<(int, string)>();

        using var _ = left.CombineLatestMatter(right).Subscribe(received.Add);

        left.OnNext(1);
        left.OnNext(2);

        Assert.Empty(received);

        right.OnNext("a");

        Assert.Single(received);
        Assert.Equal((2, "a"), received[0]);
    }

    // ── CombineLatest (triple) ────────────────────────────────────────────────

    [Fact]
    public void CombineLatestMatter_triple_emits_tuple_when_any_fires_after_all_have_emitted()
    {
        var first = new Subject<int>();
        var second = new Subject<string>();
        var third = new Subject<bool>();
        var received = new List<(int, string, bool)>();

        using var _ = first.CombineLatestMatter(second, third).Subscribe(received.Add);

        first.OnNext(1);
        second.OnNext("a");
        third.OnNext(true);

        first.OnNext(2);
        second.OnNext("b");
        third.OnNext(false);

        Assert.Equal(4, received.Count);
        Assert.Equal((1, "a", true), received[0]);
        Assert.Equal((2, "a", true), received[1]);
        Assert.Equal((2, "b", true), received[2]);
        Assert.Equal((2, "b", false), received[3]);
    }

    [Fact]
    public void CombineLatestMatter_triple_does_not_emit_until_all_three_have_fired()
    {
        var first = new Subject<int>();
        var second = new Subject<string>();
        var third = new Subject<bool>();
        var received = new List<(int, string, bool)>();

        using var _ = first.CombineLatestMatter(second, third).Subscribe(received.Add);

        first.OnNext(1);
        second.OnNext("a");

        Assert.Empty(received);

        third.OnNext(true);

        Assert.Single(received);
        Assert.Equal((1, "a", true), received[0]);
    }

    // ── WithLatestFrom (pair) ─────────────────────────────────────────────────

    [Fact]
    public void WithLatestFromMatter_pair_emits_tuple_pairing_source_with_latest_other()
    {
        var source = new Subject<int>();
        var other = new Subject<string>();
        var received = new List<(int, string)>();

        using var _ = source.WithLatestFromMatter(other).Subscribe(received.Add);

        other.OnNext("a");
        source.OnNext(1);
        other.OnNext("b");
        source.OnNext(2);

        Assert.Equal(2, received.Count);
        Assert.Equal((1, "a"), received[0]);
        Assert.Equal((2, "b"), received[1]);
    }

    [Fact]
    public void WithLatestFromMatter_pair_does_not_emit_when_other_fires()
    {
        var source = new Subject<int>();
        var other = new Subject<string>();
        var received = new List<(int, string)>();

        using var _ = source.WithLatestFromMatter(other).Subscribe(received.Add);

        other.OnNext("a");
        other.OnNext("b");
        other.OnNext("c");

        Assert.Empty(received);
    }

    [Fact]
    public void WithLatestFromMatter_pair_drops_source_emissions_before_other_has_emitted()
    {
        var source = new Subject<int>();
        var other = new Subject<string>();
        var received = new List<(int, string)>();

        using var _ = source.WithLatestFromMatter(other).Subscribe(received.Add);

        source.OnNext(1);
        source.OnNext(2);

        Assert.Empty(received);

        other.OnNext("a");
        source.OnNext(3);

        Assert.Single(received);
        Assert.Equal((3, "a"), received[0]);
    }

    // ── WithLatestFrom (triple) ───────────────────────────────────────────────

    [Fact]
    public void WithLatestFromMatter_triple_emits_tuple_pairing_source_with_latest_from_both_others()
    {
        var source = new Subject<int>();
        var other1 = new Subject<string>();
        var other2 = new Subject<bool>();
        var received = new List<(int, string, bool)>();

        using var _ = source.WithLatestFromMatter(other1, other2).Subscribe(received.Add);

        other1.OnNext("a");
        other2.OnNext(true);
        source.OnNext(1);

        other1.OnNext("b");
        source.OnNext(2);

        Assert.Equal(2, received.Count);
        Assert.Equal((1, "a", true), received[0]);
        Assert.Equal((2, "b", true), received[1]);
    }

    [Fact]
    public void WithLatestFromMatter_triple_does_not_emit_when_either_other_fires()
    {
        var source = new Subject<int>();
        var other1 = new Subject<string>();
        var other2 = new Subject<bool>();
        var received = new List<(int, string, bool)>();

        using var _ = source.WithLatestFromMatter(other1, other2).Subscribe(received.Add);

        other1.OnNext("a");
        other1.OnNext("b");
        other2.OnNext(true);
        other2.OnNext(false);

        Assert.Empty(received);
    }

    [Fact]
    public void WithLatestFromMatter_triple_drops_source_emissions_before_either_other_has_emitted()
    {
        var source = new Subject<int>();
        var other1 = new Subject<string>();
        var other2 = new Subject<bool>();
        var received = new List<(int, string, bool)>();

        using var _ = source.WithLatestFromMatter(other1, other2).Subscribe(received.Add);

        other1.OnNext("a");
        source.OnNext(1); // other2 hasn't emitted yet — should be dropped

        Assert.Empty(received);

        other2.OnNext(true);
        source.OnNext(2);

        Assert.Single(received);
        Assert.Equal((2, "a", true), received[0]);
    }
}
