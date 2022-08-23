using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine;

namespace Rzeka
{
    /* ! Events for:
     
    - new pluck, loom, weaving
     */

    [Flags]
    public enum Meow
    {
        New = 1 << 0,
        Cast = 1 << 1,
        Blocked = 1 << 2,
    }

    public class StreamEvent
    {

    }

    public class MatterReceivedStreamEvent
    {
        //public TScrollBase 
    }


    public class Eris
    {
        EventHandler<TMatter> NextMatter { get; set; }
        EventHandler<Exception> NextException { get; }
        EventHandler NextCompletion { get; }

        public Eris()
        {
            Observable.FromEventPattern<TMatter>(
                    h => NextMatter += h,
                    h => NextMatter -= h)
                .Where(pattern => pattern.Sender is TAlteringScroll);
                //.Select(pattern => new (object who, ))
        }

        public IObserver<T> GetObserver<T>(TScrollBase scroll) where T : TMatter
        {
            return Observer.Create<T>(
                onNext: val => NextMatter.Invoke(scroll, val),
                onError: err => NextException.Invoke(scroll, err),
                onCompleted: () => NextCompletion.Invoke(scroll, null));
        }
    }
}