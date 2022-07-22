/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Reactive.Disposables;

namespace RzekaRiver
{
    public class IWeaveable<T, Min, Mout> : IObservable<Mout>, IDisposable //IObserver<Min>,
        where T : Thought<Min, Mout>
        where Min : Matter
        where Mout : Matter
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IDisposable Subscribe(IObserver<Mout> observer)
        {
            throw new NotImplementedException();
        }
    }

    public class IWeaveable<T> : IObservable<T>, IDisposable where T : ThoughtBase
    {
        IObservable<T> _observable;
        private readonly object who;
        private readonly Loom<T> loom;
        private readonly IDisposable knot;

        CompositeDisposable _disposable;
        // HashSet<IObserver<T>> _observers;

        // * IWeaveable cannot be instantiated outside of
        internal IWeaveable(IObservable<T> observable, object who, Loom<T> loom)
        {
            // _observers = new HashSet<IObserver<T>>();
            _observable = observable;
            this.who = who;
            this.loom = loom;
        }

        public void Dispose()
        {
            // _disposable.Dispose();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            // * Wrapping observer
            WhoObserver<T> whoObserver = new(observer, who);

            // _observers.Add(observer);

            // TODO rework both here and inside the Do operator
            // TODO to use a special factory for the ReusableCompositeDisposable
            return new CompositeDisposable(
                    Disposable.Create(() => loom.RemoveObserver(whoObserver)),
                    _observable.Subscribe(whoObserver)
                );

            // return new CompositeDisposable(
            //     _observable.Subscribe(whoObserver),
            //     ObserverCountingDisposable(observer)
            // );
        }

        // IDisposable ObserverCountingDisposable(IObserver<T> observer)
        // {
        //     return Disposable.Create(() =>
        //         {
        //             _observers.Remove(observer);

        //             if (_observers.Count == 0)
        //             {
        //                 _disposable.Dispose();
        //             }
        //         });
        // }

        // void ClearReferences()
        // {
        //     _observers = null;
        //     _observable = null;
        //     _who = null;
        // }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */