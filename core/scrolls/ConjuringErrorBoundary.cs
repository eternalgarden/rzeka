using System;
using System.Reactive.Linq;

namespace Rzeka;

internal static class ConjuringErrorBoundary
{
    // Per-spell error boundary. Catches OnError that the user's pipeline didn't handle,
    // whispers it into Eris with full spell attribution, then completes the conjurer cleanly
    // so the river survives and other sources keep flowing.
    //
    // Contract: user-side .Catch / .Retry / .OnErrorResumeNext run first. Anything that
    // falls through reaches here.
    public static IObservable<T> WhisperOnError<T>(this IObservable<T> source, ISpell spell)
        where T : IMatter
    {
        return source.Catch<T, Exception>(ex =>
        {
            spell.Eris.PublishMessage(
                new MessageOccurence
                {
                    Guid = Guid.NewGuid(),
                    RzekaMessageType = RzekaMessageType.Horror,
                    Message =
                        $"Unhandled error in {spell.Title} (owned by {spell.Who}): {ex.Message}",
                    Exception = ex,
                    Circumstances = Array.Empty<Guid>(),
                    Timestamp = DateTimeOffset.Now,
                }
            );

            // Optional user hook. Whisper always runs first so visibility is uniform; the
            // callback is opt-in additional behavior. If it throws, the throw propagates as
            // OnError back into the pipeline — that's the crash-on-error path.
            spell.Eris.OnUnhandledSourceError?.Invoke(spell, ex);

            return Observable.Empty<T>();
        });
    }
}
