namespace Rzeka;

// TODO Add proper api method summaries after finishing the readme.
public interface IRzeka : IWhisper
{
    /// <summary>
    /// Scry is used predominantly for subscriptions between different Rzeka instances. 
    /// </summary>
    IObservable<T> Scry<T>()
        where T : IMatter;

    // Strand
    IDisposable Strand<TOut>(object who, IObservable<TOut> spell)
        where TOut : IMatter;

    // Pluck
    void Pluck<T>(object who, T matter)
        where T : IMatter;

    #region Shuttle

    IDisposable Shuttle<TIn, TOut>(object who, Func<IObservable<TIn>, IObservable<TOut>> spell)
        where TIn : IRequest
        where TOut : IResponse<TIn>;

    #endregion // Shuttle

    #region Looms

    IDisposable Loom<T1, TOut>(object who, Func<IObservable<T1>, IObservable<TOut>> spell)
            where TOut : IMatter
        where T1 : IMatter;

    IDisposable Loom<T1, T2, TOut>(
        object who,
        Func<IObservable<T1>, IObservable<T2>, IObservable<TOut>> spell
    )
        where TOut : IMatter
        where T1 : IMatter
        where T2 : IMatter;

    IDisposable Loom<T1, T2, T3, TOut>(
        object who,
        Func<IObservable<T1>, IObservable<T2>, IObservable<T3>, IObservable<TOut>> spell
    )
        where TOut : IMatter
        where T1 : IMatter
        where T2 : IMatter
        where T3 : IMatter;

    #endregion // Looms

    #region Weavings

    IDisposable Weave<T>(object who, IObserver<T> spell)
        where T : IMatter;

    IDisposable Weave<T1>(object who, Func<IObservable<T1>, IDisposable> spell)
        where T1 : IMatter;

    IDisposable Weave<T1, T2>(
        object who,
        Func<IObservable<T1>, IObservable<T2>, IDisposable> spell
    )
        where T1 : IMatter
        where T2 : IMatter;

    IDisposable Weave<T1, T2, T3>(
        object who,
        Func<IObservable<T1>, IObservable<T2>, IObservable<T3>, IDisposable> spell
    )
        where T1 : IMatter
        where T2 : IMatter
        where T3 : IMatter;

    #endregion // Weavings
}
