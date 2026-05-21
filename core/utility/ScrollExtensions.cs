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
    /// Performs a side effect for each emission without breaking the chain.
    /// Use inside Loom lambdas to signal intentional component reactions, not debug taps.
    /// </summary>
    public static IObservable<T> Reacting<T>(this IObservable<T> source, Action<T> reaction) =>
        source.Do(reaction);

    /// <summary>
    /// Runs a side effect on each emission and produces output matter in one explicit step.
    /// Sugar over .Do() + .Select() — signals the reaction happens inside the chain, not as a debug tap.
    /// </summary>
    public static IObservable<TOut> Reacting<T, TOut>(
        this IObservable<T> source,
        Func<T, TOut> reaction
    ) => source.Select(reaction);

    // -------------
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 26 January 2023 🌊 */
