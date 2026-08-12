using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rzeka.Tests;

public class PluckTests
{
    sealed class Ink : Matter { }

    static SpringRiver NewRiver() => (SpringRiver)new Spring().Create("test", ImmediateScheduler.Instance);

    // Builds a river whose "main thread" is a real event loop, so that marshalling is observable.
    // Spring.Create captures the calling thread, so it must be constructed on the loop itself.
    static (SpringRiver River, EventLoopScheduler Loop, int MainThreadId) NewLoopRiver()
    {
        var loop = new EventLoopScheduler();
        SpringRiver? river = null;
        int mainThreadId = 0;
        var ready = new ManualResetEventSlim(false);

        loop.Schedule(() =>
        {
            mainThreadId = Environment.CurrentManagedThreadId;
            river = (SpringRiver)new Spring().Create("test", loop);
            ready.Set();
        });

        Assert.True(ready.Wait(5000), "river was never created on the loop thread");
        return (river!, loop, mainThreadId);
    }

    // ── Lifecycle ────────────────────────────────────────────────────────────

    [Fact]
    public void Pluck_emits_Created_and_Forgotten_spell_occurences()
    {
        // Pluck is one-shot — Created → Shaped → Forgotten all fire synchronously in the constructor
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        river.Pluck(new object(), new Ink());

        Assert.Contains(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Plucking
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
    public void Pluck_does_not_emit_HasMana_or_NoMana()
    {
        // Pluck is excluded from mana tracking — it is instantaneous, not a sustained source
        var river = NewRiver();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        river.Pluck(new object(), new Ink());

        Assert.DoesNotContain(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Plucking
                && o.SpellOccurenceCategory == SpellOccurenceCategory.HasMana
        );
        Assert.DoesNotContain(
            spellOccurences,
            o =>
                o.Source.SpellSchool == SpellSchool.Plucking
                && o.SpellOccurenceCategory == SpellOccurenceCategory.NoMana
        );
    }

    [Fact]
    public void Pluck_attributes_spell_occurences_to_who()
    {
        var river = NewRiver();
        var who = new object();
        var spellOccurences = new List<SpellOccurence>();
        river.Eris.SpellOccurences.Subscribe(spellOccurences.Add);

        river.Pluck(who, new Ink());

        Assert.All(
            spellOccurences,
            o => Assert.Same(who, o.Source.Who)
        );
    }

    // ── Emission ─────────────────────────────────────────────────────────────

    [Fact]
    public void Pluck_emits_matter_occurence_with_Shaped_category_attributed_to_who()
    {
        var river = NewRiver();
        var who = new object();
        var ink = new Ink();
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        river.Pluck(who, ink);

        MatterOccurence emitted = Assert.Single(matterOccurences);
        Assert.Equal(ink.Guid, emitted.Matter.Guid);
        Assert.Equal(MatterOccurenceCategory.Shaped, emitted.MatterOccurenceCategory);
        Assert.Equal(SpellSchool.Plucking, emitted.Source.SpellSchool);
        Assert.Same(who, emitted.Source.Who);
        Assert.False(emitted.ManualCircumstances);
    }

    [Fact]
    public void Pluck_routes_matter_to_Scry_subscribers()
    {
        var river = NewRiver();
        var received = new List<Ink>();
        river.Scry<Ink>().Subscribe(received.Add);

        var ink = new Ink();
        river.Pluck(new object(), ink);

        IMatter item = Assert.Single(received);
        Assert.Equal(ink.Guid, item.Guid);
    }

    [Fact]
    public void Pluck_preserves_manual_circumstances_and_marks_manual()
    {
        var river = NewRiver();
        var ctx1 = new Ink();
        var ctx2 = new Ink();
        Ink ink = new Ink().WithCircumstances<Ink>(ctx1, ctx2);
        var matterOccurences = new List<MatterOccurence>();
        river.Eris.MatterOccurences.Subscribe(matterOccurences.Add);

        river.Pluck(new object(), ink);

        MatterOccurence emitted = Assert.Single(matterOccurences);
        Assert.True(emitted.ManualCircumstances);
        Assert.Equal(2, emitted.Matter.Circumstances.Count);
        Assert.Contains(emitted.Matter.Circumstances, c => c.Guid == ctx1.Guid);
        Assert.Contains(emitted.Matter.Circumstances, c => c.Guid == ctx2.Guid);
    }

    // ── Main-thread marshalling ──────────────────────────────────────────────

    [Fact]
    public void Pluck_on_the_main_thread_stays_synchronous()
    {
        // The on-thread path must not be queued: downstream Weaves are expected to have
        // already run by the time Pluck returns.
        var river = NewRiver();
        bool observed = false;
        using var weave = river.Weave<Ink>("watcher", src => src.Subscribe(_ => observed = true));

        river.Pluck(new object(), new Ink());

        Assert.True(observed);
    }

    [Fact]
    public void Pluck_from_a_worker_thread_is_marshalled_onto_the_main_thread()
    {
        var (river, loop, mainThreadId) = NewLoopRiver();
        using (loop)
        {
            int publishedOn = 0;
            var arrived = new ManualResetEventSlim(false);
            using var weave = river.Weave<Ink>(
                "watcher",
                src => src.Subscribe(_ =>
                {
                    publishedOn = Environment.CurrentManagedThreadId;
                    arrived.Set();
                })
            );

            int pluckedFrom = 0;
            Task.Run(() =>
            {
                pluckedFrom = Environment.CurrentManagedThreadId;
                river.Pluck(new object(), new Ink());
            }).Wait(5000);

            Assert.True(arrived.Wait(5000), "the marshalled pluck never published");
            Assert.NotEqual(mainThreadId, pluckedFrom);   // it really was off-thread
            Assert.Equal(mainThreadId, publishedOn);      // but it published on the main thread
        }
    }

    [Fact]
    public void Pluck_from_a_worker_thread_no_longer_whispers_an_off_thread_Horror()
    {
        var (river, loop, _) = NewLoopRiver();
        using (loop)
        {
            var messages = new List<SerializableMessageOccurence>();
            river.Eris.SerializableMessageOccurences.Subscribe(m =>
            {
                lock (messages) messages.Add(m);
            });

            var arrived = new ManualResetEventSlim(false);
            using var weave = river.Weave<Ink>("watcher", src => src.Subscribe(_ => arrived.Set()));

            Task.Run(() => river.Pluck(new object(), new Ink())).Wait(5000);
            Assert.True(arrived.Wait(5000), "the marshalled pluck never published");

            lock (messages)
            {
                Assert.DoesNotContain(
                    messages,
                    m => m.messageType == RzekaMessageType.Horror
                        && m.message.Contains("Off-thread matter")
                );
                // ...but the design smell is still surfaced, as a Hunch.
                Assert.Contains(
                    messages,
                    m => m.messageType == RzekaMessageType.Hunch
                        && m.message.Contains("Off-thread pluck")
                );
            }
        }
    }

    [Fact]
    public void Pluck_from_a_worker_thread_still_delivers_the_matter_intact()
    {
        var (river, loop, _) = NewLoopRiver();
        using (loop)
        {
            var context = new Ink();
            Ink sent = new Ink().WithCircumstances<Ink>(context);

            Ink? received = null;
            var arrived = new ManualResetEventSlim(false);
            using var weave = river.Weave<Ink>(
                "watcher",
                src => src.Where(m => m.Guid == sent.Guid).Subscribe(m =>
                {
                    received = m;
                    arrived.Set();
                })
            );

            Task.Run(() => river.Pluck(new object(), sent)).Wait(5000);

            Assert.True(arrived.Wait(5000), "the marshalled pluck never published");
            Assert.NotNull(received);
            Assert.Equal(sent.Guid, received!.Guid);
            Assert.Contains(received.Circumstances, c => c.Guid == context.Guid);
        }
    }
}
