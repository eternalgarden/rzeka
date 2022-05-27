/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;

namespace Rzeka
{
    public static partial class Observer
    {
        // -------------
        
        internal static IObserver<T> CreateSubscribeObserver<T>(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            // need compare for avoid iOS AOT
            if (onNext == Stubs<T>.Ignore)
            {
                return new Subscribe_<T>(onError, onCompleted);
            }
            else
            {
                return new SubscribeObserver<T>(onNext, onError, onCompleted);
            }
        }

        internal static IObserver<T> CreateSubscribeWithStateObserver<T, TState>(TState state, Action<T, TState> onNext, Action<Exception, TState> onError, Action<TState> onCompleted)
        {
            return new Subscribe<T, TState>(state, onNext, onError, onCompleted);
        }
        
        // -------------
    }
}