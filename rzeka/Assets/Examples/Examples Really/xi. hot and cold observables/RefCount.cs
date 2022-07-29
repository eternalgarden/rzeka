using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;
using System.Threading;
using Rzeka.Unirx;

namespace Rzeka.Examples
{
    public class RefCount : LoomingMono
    {
        /*
         * https://reactivex.io/documentation/operators/refcount.html
         * 
         * Simplifies the process of connecting and disconnecting a published / "connected observable"
         * Does ot for you
         * Usefull when you don't care when the observable starts emitting to its observers
         * 
         * "When the first observer subscribes to this Observable, RefCount connects to the underlying 
         * connectable Observable. RefCount then keeps track of how many other observers subscribe to
         * it and does not disconnect from the underlying connectable Observable until the last observer 
         * has done so."
         * 
         * Notice it disconnects when it has no more observers.
         * 
         * https://kau.sh/blog/rxjava-tip-for-the-day-share-publish-refcount-and-all-that-jazz/
         * “ConnectedObservable” - This is a kind of observable which doesn’t emit items even if subscribed to. 
         * It only starts emitting items after its .connect() method is called.
         * ...
         * IMPORTANT!
         * It is for this reason that a connected obesrvable is also considered “cold” or “inactive” before 
         * the connect method is invoked.
         * 
         */

        [InfoBox("A perfect example of using RefCount is inside this reactive property class")]
        [SerializeField] InspectableReactiveProperty<int> _someInt;


        [SerializeField] bool RunPlaymodeExample = true;

        protected override void OnEnable()
        {
            if (RunPlaymodeExample is false) return;
            else
            {
                base.OnEnable();

                q += UnityObservable
                    .EveryUpdate()
                    .Where(_ => Input.GetKeyDown(KeyCode.Return))
                    .Subscribe(_ =>
                    {
                        _someInt.Value = UnityEngine.Random.Range(0, 100);
                    });

                q += _someInt
                    .Subscribe(i => Debug.Log(i));
            }
        }
    }
}
