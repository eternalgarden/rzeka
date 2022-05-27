/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;

namespace Rzeka.Operators
{
    /* 🌊 ---- ---- */

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