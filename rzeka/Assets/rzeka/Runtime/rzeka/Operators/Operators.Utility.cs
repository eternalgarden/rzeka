/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    public static partial class Observable
    {
        //
        // ⛺ ─── Do & It's Variants ───────────────────────────────────────────────────
        //
        #region Do & It's Variants

        /// <summary>
        /// The most basic creation of a stream side-effect, basically copying an observable stream 
        /// to a given observer, but if you have an Observer you really should just subscribe
        /// to an observable stream since IObservable.Subscribe() method takes exactly that
        /// as it's single parameter, an IObserver.
        /// 
        /// Side effects can be really only sensibly used in archtectural decisions for
        /// example when creating loggers, debuggers or middleware, but the last one is
        /// already questionable as putting dream logic (commonly referred to as 'business'
        /// logic) is widely disputable. See desicussions on microservices and even
        /// 'Introduction to Rx' book whebn it speaks of side-effects here: http://introtorx.com/Content/v1.0.10621.0/09_SideEffects.html#SideEffects
        /// 
        /// Avoid unless you REALLY know what you are doing, otherwise it most likely means 
        /// you don't fully get the concept of rx programming (I avoid using 'reactive'
        /// naming since I find it terribly unfortunate for the idea behind it, like guys,
        /// I know pull-oriented ontology of Enumerables wasn't the best idea, but it's
        /// not such a great of a trauma to name an entirely new paradigm as doing it's opposite - 'reacting',
        /// 'observing' is such a better word and fortunately it describes the foundational
        /// interfaces of rx).
        /// </summary>
        /// <param name="source">Source observable</param>
        /// <param name="observer">Middleware observer</param>
        /// <returns>An unchanged, up-the-stream parent IObservable</returns>
        public static IObservable<T> Do<T>(this IObservable<T> source, IObserver<T> observer)
        {
            return new DoMiddlewareObservable<T>(source, observer);
        }

        /// <summary>
        /// Just a subscriber to create side effects like logging, don't use this for
        /// dream logic. See the other overload of this method to read a longer explanation.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="onNext"></param>
        /// <param name="onError"></param>
        /// <param name="onCompleted"></param>
        /// <param name="onSubscribed"></param>
        /// <param name="onUnsubscribed"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObservable<T> Do<T>(this IObservable<T> source,
            Action<T> onNext = null,
            Action<Exception> onError = null,
            Action onCompleted = null,
            Action<IObserver<T>> onSubscribed = null,
            Action<IObserver<T>> onUnsubscribed = null)
        {
            return new DoObservable<T>(source, onNext, onError, onCompleted, onSubscribed, onUnsubscribed);
        }

        #endregion // ---------------------------------- Do & It's Variants -------------------------
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 29 June 2022 🌊 */