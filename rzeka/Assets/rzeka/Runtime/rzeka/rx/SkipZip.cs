using System;
using System.Reactive.Linq;

namespace Rzeka.Operators
{
    public static class Operators
    {
        // public static IObservable<(T,Y)> SkipZip<T,Y>(this IObservable<T> source,
        //     IObservable<Y> other1,
        //     Func<T, T, bool> predicate)
        // {
        //     Observable.Create()
        //     
        //     return Observable.Create<T>(observer =>
        //     {
        //         return source.Subscribe(value =>
        //         {
        //             if (!emitted || predicate(lastEmitted, value))
        //             {
        //                 observer.OnNext(value);
        //                 lastEmitted = value;
        //                 emitted = true;
        //             }
        //         }, observer.OnError, observer.OnCompleted);
        //     });
        // }
    }
}

