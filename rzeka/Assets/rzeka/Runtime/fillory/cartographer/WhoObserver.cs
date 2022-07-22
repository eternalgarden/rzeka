/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Threading;
using UnityEngine;

namespace RzekaRiver
{
    public class WhoObserver<T> : IObserver<T>, IDisposable
    {
        IObserver<T> _observer;
        object _who;
        int isStopped = 0;

        public object Who => _who;
        public Type StrandType => typeof(T);

        public WhoObserver(IObserver<T> observer, object who)
        {
            _observer = observer;
            _who = who;
            Debug.Log($"who observer created> {who.GetType()}");
        }

        
        //
        // ⛺ ─── IObseerver<T> Interface ───────────────────────────────────────────────────
        //
        #region IObseerver<T> Interface
        
        public void OnNext(T value)
        {
            if (isStopped == 0)
            {
                _observer.OnNext(value);
            }
        }

        public void OnError(Exception error)
        {
            if (Interlocked.Increment(ref isStopped) == 1)
            {
                _observer.OnError(error);
            }
        }

        public void OnCompleted()
        {
            if (Interlocked.Increment(ref isStopped) == 1)
            {
                _observer.OnCompleted();
            }
        }
        
        #endregion // ---------------------------------- IObseerver<T> Interface -------------------------

        public bool WhoAsGameObject(out object who, out GameObject whoGameobject)
        {
            who = _who;

            if (who is GameObject)
            {
                whoGameobject = who as GameObject;
                return true;
            }
            else
            {
                whoGameobject = null;
                return false;
            }
        }

        public void Dispose()
        {
            _observer = null;
            _who = null;
        }
    }
}