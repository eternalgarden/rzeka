/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/


using System;
using System.Collections;
using System.Collections.Generic;

namespace Rzeka
{
    public abstract class Any : IEnumerable<Type>
    {
        protected abstract Type[] Types { get; }

        public IEnumerator<Type> GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Types.GetEnumerator();
        }
    }

    public class Any<T, Y> : Any
        where T : TMatter
        where Y : TMatter
    {
        readonly Type[] types = new[] { typeof(T), typeof(Y) };

        protected override Type[] Types => types;
    }

}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 24 January 2023 🌊 */