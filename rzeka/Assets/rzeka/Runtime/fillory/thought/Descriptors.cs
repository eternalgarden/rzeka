/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace RzekaRiver
{
    /* 🌊 ---- ---- */

    public interface ICustomSubjectProvideable<T> where T : ThoughtBase
    {
        public ISubject<ThoughtBase> CreateCustomSubject<T>();
    }

    public interface HasDefault<TThought, YMatter> 
        where TThought : Thought<YMatter>
        where YMatter : Matter
    {
        YMatter Default { get; }

        public void SetToDefault(TThought thought, YMatter matter)
        {
            thought.Initialize(
                matter: matter,
                context: Roots.CORE,
                circumstances: Roots.NULL);
        }
    }

    public interface HasBuffer
    {
        int BufferSize { get; }
    }

    public interface TakeOnlyUnique<T> where T : Matter
    {
        bool IsDifferent(T previous, T next);
    }

    // public interface HasPool<T> where T : Matter
    // {
    //     int PoolSize { get; }
    //     void Reset(T newMatter);
    // }
    
    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 15 July 2022 🌊 */