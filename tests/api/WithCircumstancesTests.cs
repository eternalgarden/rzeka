using System.Linq;

namespace Rzeka.Tests;

public class WithCircumstancesTests
{
    sealed class Ink : Matter { }

    sealed class Quill : Matter { }

    // ── Stamping ─────────────────────────────────────────────────────────────

    [Fact]
    public void Stamping_returns_a_clone_and_leaves_the_original_untouched()
    {
        var cause = new Quill();
        var ink = new Ink();

        Ink stamped = ink.WithCircumstances(cause);

        Assert.NotSame(ink, stamped);
        Assert.Empty(ink.Circumstances);
        Assert.Equal([cause], stamped.Circumstances);
    }

    [Fact]
    public void Stamping_replaces_existing_circumstances()
    {
        var first = new Quill();
        var second = new Quill();

        Ink stamped = new Ink().WithCircumstances(first).WithCircumstances(second);

        Assert.Equal([second], stamped.Circumstances);
    }

    // ── Nothing to stamp ─────────────────────────────────────────────────────

    [Fact]
    public void No_circumstances_returns_the_same_matter()
    {
        var ink = new Ink();

        Assert.Same(ink, ink.WithCircumstances());
    }

    [Fact]
    public void No_circumstances_keeps_an_existing_stamp()
    {
        var cause = new Quill();
        Ink stamped = new Ink().WithCircumstances(cause);

        Ink restamped = stamped.WithCircumstances();

        Assert.Equal([cause], restamped.Circumstances);
    }

    // ── Nulls ────────────────────────────────────────────────────────────────

    [Fact]
    public void Null_array_is_treated_as_nothing_to_stamp()
    {
        var cause = new Quill();
        Ink stamped = new Ink().WithCircumstances(cause);

        Ink restamped = stamped.WithCircumstances((IMatter?[]?)null);

        Assert.Same(stamped, restamped);
        Assert.Equal([cause], restamped.Circumstances);
    }

    [Fact]
    public void Null_cause_is_treated_as_nothing_to_stamp()
    {
        Quill? noCause = null;
        var ink = new Ink();

        Ink stamped = ink.WithCircumstances(noCause);

        Assert.Same(ink, stamped);
        Assert.Empty(stamped.Circumstances);
    }

    [Fact]
    public void Null_causes_are_skipped_among_real_ones()
    {
        var cause = new Quill();

        Ink stamped = new Ink().WithCircumstances(null, cause, null);

        Assert.Equal([cause], stamped.Circumstances);
        Assert.True(stamped.IsCircumstancedBy(cause));
    }

    // ── Interface method ─────────────────────────────────────────────────────

    [Fact]
    public void Interface_method_behaves_like_the_extension()
    {
        var cause = new Quill();
        IMatter ink = new Ink();

        Assert.Same(ink, ink.WithCircumstances<IMatter>(null, null));
        Assert.Equal([cause], ink.WithCircumstances<IMatter>(null, cause).Circumstances.ToArray());
    }
}
