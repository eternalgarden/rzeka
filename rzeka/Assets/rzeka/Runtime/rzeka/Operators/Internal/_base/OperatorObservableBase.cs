/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    /*

    * The premises of implementing Operators in UniRx
    TODO Compare to RxNET in some time, is it just copied or reconceptualized.
    TODO Do we really need all that object inestantiation for each of the operators (internal observer implementations)
    TODO ie. couldn't it be more functional

    * 1) Operators are IObservables at their core (implement .Subscribe())
    * 2) They extend default Observable with an additional subscribe implementation (.SubscribeCore())
        ? 2.a Implementation of .SubscribeCore varies greatly between operators
        ? 2.b Almost always it is however the place where an instance of a nested 
        ?     observer (look 3.) is created and being given a refernce to the
        ?     external subscribing observer.
    * 3) They all define a nested Observer 
        ? 3.a There is a big difference between Creation and other types of Operators

    */

    // original neuecc comment:
    // implements note : all field must be readonly.
    public abstract class OperatorObservableBase<T> : IObservable<T>, IOptimizedObservable<T>
    {
        readonly bool isRequiredSubscribeOnCurrentThread;

        public OperatorObservableBase(bool isRequiredSubscribeOnCurrentThread)
        {
            this.isRequiredSubscribeOnCurrentThread = isRequiredSubscribeOnCurrentThread;
        }

        bool IOptimizedObservable<T>.IsRequiredSubscribeOnCurrentThread()
        {
            return isRequiredSubscribeOnCurrentThread;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="observer"></param>
        /// <returns>SingleAssignmentDisposable</returns>
        IDisposable IObservable<T>.Subscribe(IObserver<T> observer)
        {
            var subscription = new SingleAssignmentDisposable();

            /* original neuecc comment

             note:
             does not make the safe observer, it breaks exception durability.
             var safeObserver = Observer.CreateAutoDetachObserver<T>(observer, subscription);
             
            */

            /*

            TODO QUESTION
            
            1. The schedulers thing below.

            2. Is 'subscription' disposed aswell on Dispose call from OperatorObserverBase?

            */

            if (isRequiredSubscribeOnCurrentThread && Scheduler.IsCurrentThreadSchedulerScheduleRequired)
            {
                Scheduler.CurrentThread.Schedule(() => subscription.Disposable = SubscribeCore(observer, subscription));
            }
            
            else
            {
                subscription.Disposable = SubscribeCore(observer, subscription);
            }

            return subscription;
        }

        /// <remarks>
        /// Called by it's base class public interface call to Subscribe
        /// </remarks>
        protected abstract IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel);
    }

    /* ---- ---- ⛺ */
}
/* maria aurelia at 25 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */