/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rzeka
{
    internal static class Stubs
    {
        public static readonly Action Nop = () => { };
        public static readonly Action<Exception> Throw = ex => { ex.Throw(); };

        // marker for CatchIgnore and Catch avoid iOS AOT problem.
        public static IObservable<TSource> CatchIgnore<TSource>(Exception ex)
        {
            return Observable.Empty<TSource>();
        }
    }

    internal static class Stubs<T>
    {
        public static readonly Action<T> Ignore = (T t) => { };
        public static readonly Func<T, T> Identity = (T t) => t;
        public static readonly Action<Exception, T> Throw = (ex, _) => { ex.Throw(); };
    }

    internal static class Stubs<T1, T2>
    {
        public static readonly Action<T1, T2> Ignore = (x, y) => { };
        public static readonly Action<Exception, T1, T2> Throw = (ex, _, __) => { ex.Throw(); };
    }


    internal static class Stubs<T1, T2, T3>
    {
        public static readonly Action<T1, T2, T3> Ignore = (x, y, z) => { };
        public static readonly Action<Exception, T1, T2, T3> Throw = (ex, _, __, ___) => { ex.Throw(); };
    }
}