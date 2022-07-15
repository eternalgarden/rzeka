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
    public class RzekaObserver<T> : IObserver<T>, IDisposable
    {
        Action<T> _onNext;
        Action<Exception> _onError;
        Action _onCompleted;
        object _who;
        int isStopped = 0;

        public object Who => _who;
        public Type StrandType => typeof(T);

        public RzekaObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted, object who)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
            _who = who;
        }

        public void OnNext(T value)
        {
            if (isStopped == 0)
            {
                _onNext(value);
            }
        }

        public void OnError(Exception error)
        {
            if (Interlocked.Increment(ref isStopped) == 1)
            {
                _onError(error);
            }
        }


        public void OnCompleted()
        {
            if (Interlocked.Increment(ref isStopped) == 1)
            {
                _onCompleted();
            }
        }

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
            _onNext = null;
            _onCompleted = null;
            _onError = null;
            _who = null;
        }
    }
}