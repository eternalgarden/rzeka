/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Rzeka
{
    public static partial class Observer
    {
        // -------------
        
        public static IObserver<T> Create<T>(Action<T> onNext)
        {
            return Create<T>(onNext, Rzeka.Stubs.Throw, Rzeka.Stubs.Nop);
        }

        public static IObserver<T> Create<T>(Action<T> onNext, Action<Exception> onError)
        {
            return Create<T>(onNext, onError, Rzeka.Stubs.Nop);
        }

        public static IObserver<T> Create<T>(Action<T> onNext, Action onCompleted)
        {
            return Create<T>(onNext, Rzeka.Stubs.Throw, onCompleted);
        }

        public static IObserver<T> Create<T>(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            // need compare for avoid iOS AOT
            if (onNext == Stubs<T>.Ignore)
            {
                return new EmptyOnNextAnonymousObserver<T>(onError, onCompleted);
            }
            else
            {
                return new AnonymousObserver<T>(onNext, onError, onCompleted);
            }
        }
        
        // -------------
    }
}