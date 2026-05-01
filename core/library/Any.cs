/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Rzeka;
public abstract class Any : IEnumerable<Type>
{
    protected abstract IEnumerable<Type> Types { get; }

    public IEnumerator<Type> GetEnumerator()
    {
        return Types.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Types.GetEnumerator();
    }
}

public class Any<TOne, TTwo> : Any
    where TOne : IMatter
    where TTwo : IMatter
{
    readonly Type[] _types = { typeof(TOne), typeof(TTwo) };

    protected override IEnumerable<Type> Types => _types;
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 24 January 2023 🌊 */

