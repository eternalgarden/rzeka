/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/

using System;
using System.Reactive.Linq;

namespace Rzeka.Unirx
{
    public static class UniRxExtensions
    {
        public static IObservable<T> ObserveOnMainThread<T>(this IObservable<T> source)
        {
            return source.ObserveOn(UnityMainThreadScheduler.Instance);
        }
    }
}

/* created at 2025-04-9, Wed, 9:44 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */