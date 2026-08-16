/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/

using System.Reactive.Linq;

namespace Rzeka;
public static class ScrollExtensions
{
    // -------------

    /// <summary>
    /// Rzeka tuple shorthand for Rx CombineLatest.
    /// Combines two streams into a tuple, emitting whenever either fires.
    /// Use when both streams are triggers - e.g. a display that updates when health OR shield changes.
    /// Named to avoid ambiguity with ObservableEx overloads that share the same parameter signature.
    /// </summary>
    public static IObservable<(T1, T2)> CombineLatestMatter<T1, T2>(
        this IObservable<T1> source,
        IObservable<T2> other
    ) => source.CombineLatest(other, (a, b) => (a, b));

    /// <summary>
    /// Rzeka tuple shorthand for Rx CombineLatest.
    /// Combines three streams into a tuple, emitting whenever any fires.
    /// Use when all streams are triggers - e.g. a display that updates when health, shield OR stamina changes.
    /// Named to avoid ambiguity with ObservableEx overloads that share the same parameter signature.
    /// </summary>
    public static IObservable<(T1, T2, T3)> CombineLatestMatter<T1, T2, T3>(
        this IObservable<T1> source,
        IObservable<T2> second,
        IObservable<T3> third
    ) => source.CombineLatest(second, third, (a, b, c) => (a, b, c));

    /// <summary>
    /// Rzeka tuple shorthand for Rx WithLatestFrom.
    /// Pairs each emission of source with the latest value from other.
    /// Use when source is the trigger and other is just "what's the current state of X".
    /// Note: silently drops source emissions that arrive before other has emitted.
    /// Named to avoid ambiguity with ObservableEx.WithLatestFrom which shares the same parameter signature.
    /// </summary>
    public static IObservable<(T1, T2)> WithLatestFromMatter<T1, T2>(
        this IObservable<T1> source,
        IObservable<T2> other
    ) => source.WithLatestFrom(other, (a, b) => (a, b));

    /// <summary>
    /// Rzeka tuple shorthand for Rx WithLatestFrom.
    /// Pairs each emission of source with the latest values from other1 and other2.
    /// Use when source is the trigger and others are just "what's the current state of X".
    /// Note: silently drops source emissions that arrive before either other has emitted.
    /// Named to avoid ambiguity with ObservableEx.WithLatestFrom which shares the same parameter signature.
    /// </summary>
    public static IObservable<(T1, T2, T3)> WithLatestFromMatter<T1, T2, T3>(
        this IObservable<T1> source,
        IObservable<T2> other1,
        IObservable<T3> other2
    ) =>
        source
            .WithLatestFrom(other1, (a, b) => (a, b))
            .WithLatestFrom(other2, (ab, c) => (ab.a, ab.b, c));

    /// <summary>
    /// Performs an effect for each emission without breaking the chain.
    /// Use ONLY when the effect is what makes the emitted matter true — i.e. the matter
    /// would be a lie without it. An effect that is merely a consequence of matter already
    /// true belongs in a Weave, which records a spell occurrence; this does not.
    /// Note it re-runs per attempt inside SelectMany inner sequences and under .Retry.
    /// A Loom that performs work and then reports it usually wants to be a Shuttle.
    /// </summary>
    public static IObservable<T> Perform<T>(this IObservable<T> source, Action<T> effect) =>
        source.Do(effect);

    /// <summary>
    /// Performs an effect and produces the resulting matter in one explicit step.
    /// Sugar over .Do() + .Select(). Same rule as the Action overload: only for effects the
    /// emitted matter's truth depends on — consequences belong in a Weave.
    /// </summary>
    public static IObservable<TOut> Perform<T, TOut>(
        this IObservable<T> source,
        Func<T, TOut> effect
    ) => source.Select(effect);

    // -------------
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 26 January 2023 🌊 */
