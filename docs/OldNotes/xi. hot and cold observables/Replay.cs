using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Rzeka.Examples
{
    public class Replay : LoomingMono
    {
        [Button(".Replay()")]
        async void ReplayOperator()
        {
            CollectibleDisposable c = new();

            var second = TimeSpan.FromSeconds(1);
            var hot = Observable.Interval(second)
                //.Take(3)
                .Publish();

            // * note that the order of the two below doesnt matter
            // * this is because .Interval will only emit first item after an entire second only
            c += hot.Connect(); // tuuut tuuuut ! train is not waiting
            c += hot.Subscribe(i => Debug.Log($"<color=white>{i}</color>"));

            await Task.Delay(second);
            //Thread.Sleep(second); //Run hot and ensure a value is lost. -- 0

            var observable = hot;
            //var observable = hot.Replay(3);

            //c += observable.Connect();

            c += observable.Subscribe(i => Debug.Log($"<color=yellow>first subscription : {i}</color>"));

            await Task.Delay(second);

            //Thread.Sleep(second); // -- 1

            c += observable.Subscribe(i => Debug.Log($"<color=blue>second subscription : {i}</color>"));

            await Task.Delay(second);
            await Task.Delay(second);

            //Thread.Sleep(second); // -- 2
            //Thread.Sleep(second); // -- 3

            c+= observable.Subscribe(i => Debug.Log($"<color=cyan>third subscription : {i}</color>"));
            
            c.Dispose();
        }
    }
}
