using System;
using System.Reactive.Linq;

namespace Rzeka;

internal static class ConjuringErrorBoundary
{
    const string WhisperedMark = "rzeka.whispered";

    // Per-spell error boundary. Catches OnError that the user's pipeline didn't handle,
    // whispers it into Eris with full spell attribution, then completes the conjurer cleanly
    // so the river survives and other sources keep flowing.
    //
    // Contract: user-side .Catch / .Retry / .OnErrorResumeNext run first. Anything that
    // falls through reaches here.
    //
    // `trigger` returns the matter the spell was handling when it failed (a Loom's latest
    // inputs, a Shuttle's latest request). Exact for synchronous lambdas; after an await it is
    // only the latest input, not necessarily the one that failed.
    public static IObservable<T> WhisperOnError<T>(
        this IObservable<T> source,
        ISpell spell,
        Func<IMatter[]>? trigger = null
    )
        where T : IMatter
    {
        return source.Catch<T, Exception>(ex =>
        {
            IMatter[] triggeringMatter = trigger?.Invoke() ?? Array.Empty<IMatter>();

            Whisper(spell, ex, triggeringMatter);

            // If the callback throws, the throw propagates as OnError back into the pipeline —
            // that's the crash-on-error path.
            InvokeUserCallback(spell, ex);

            return Observable.Empty<T>();
        });
    }

    // Weave boundary: whisper and contain. Without it, a Weave's subscriber throwing propagates
    // up to whoever pushed the matter, bypassing the conjuring boundary above, and on its way
    // faults Stream<T>'s CurrentThread-scheduled feeder, which then silently drops every later
    // emission of that matter type for all subscribers. Containing it here keeps the stream
    // alive for everyone else. The failing Weave's own subscription still ends (Rx unsubscribes
    // an observer that throws), mirroring how a failing source completes.
    //
    // The user callback runs here as well, so crash-on-error stays opt-in for Weaves too. A
    // throw from the callback is let through deliberately (and faults the stream, as a crash
    // would).
    public static IObservable<T> WhisperOnThrow<T>(this IObservable<T> source, ISpell spell)
        where T : IMatter
    {
        return Observable.Create<T>(observer =>
            source.Subscribe(
                matter =>
                {
                    try
                    {
                        observer.OnNext(matter);
                    }
                    catch (Exception ex) when (!ex.Data.Contains(WhisperedMark))
                    {
                        Whisper(spell, ex, new IMatter[] { matter });
                        InvokeUserCallback(spell, ex);
                    }
                },
                observer.OnError,
                observer.OnCompleted
            )
        );
    }

    // Marked exceptions were already whispered, or are a deliberate crash from the user
    // callback. Weave boundaries further up the stack let them pass instead of containing them.
    static void InvokeUserCallback(ISpell spell, Exception ex)
    {
        try
        {
            spell.Eris.OnUnhandledSourceError?.Invoke(spell, ex);
        }
        catch (Exception crash)
        {
            crash.Data[WhisperedMark] = true;
            throw;
        }
    }

    static void Whisper(ISpell spell, Exception ex, IMatter[] trigger)
    {
        ex.Data[WhisperedMark] = true;

        spell.Eris.PublishMessage(
            new MessageOccurence
            {
                Guid = Guid.NewGuid(),
                RzekaMessageType = RzekaMessageType.Horror,
                Message =
                    $"Unhandled error in {spell.Title} (owned by {spell.Eris.DescribeWho(spell)}): {ex.Message}",
                Exception = ex,
                Circumstances = Array.ConvertAll(trigger, m => m.Guid),
                Timestamp = DateTimeOffset.Now,
            }
        );

        spell.Eris.PublishMiscast(new Miscast(spell, ex, trigger));
    }
}
