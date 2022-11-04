/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Reactive.Linq;
using UnityEngine;

namespace Rzeka
{
    public static class RzekaExtensions
    {
        // -------------
    
        public static IObservable<T> Register<T>(this IObservable<T> source)
            where T : TMatter
        {
            // source = source.Where(x => x is not null);
            return source
                .Publish()
                .RefCount();
        }
    
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 04 November 2022 🌊 */