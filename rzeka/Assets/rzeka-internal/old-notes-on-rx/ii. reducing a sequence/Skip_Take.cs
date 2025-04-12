using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka.Examples
{
    public class Skip_Take : LoomingMono
    {


        [Button(".SkipLast()")]
        void SkipLast()
        {
            /*
             * Example taken straight from introtorx
             * http://introtorx.com/Content/v1.0.10621.0/05_Filtering.html#SkipLastTakeLast
             * 
             * In case like me, you are not a Highly Advanced Inteligen LIfeform and you are
             * boggled for half an hour how on earth this thing * k n o w s * to start publishing
             * 1,2 and 3 before it knows the final length of the sequence...
             * 
             * SkipLast uses a queue under the hood:
             * 
                public override void OnNext(TSource value)
                {
                    _queue.Enqueue(value);

                    if (_queue.Count > _count)
                    {
                        OnNext(_queue.Dequeue());
                    }
                }
             *
             * So unfortunately there is no magic there contrary to how my mind baffled for a brief,
             * simply once queue has enough items to be more than a requested _count (in our case 2), 
             * it can safely assume that one by one it can emit those items without waiting for the 
             * actual sequence to end. Then since it has a 'lag' of exactly that same _count length,
             * it will have 'time' to stop emitting the last _count items if the sequence hits an
             * .OnCompleted().. Clevar!
             * 
             * Also might very well be that my 'explanation' made it only dimmer, in that case I wish
             * you fun boggling over it.
             */

            var subject = new Subject<int>();

            subject
                .SkipLast(2)
                .Subscribe(e => Debug.Log(e), () => Debug.Log("Completed"));

            Debug.Log("Pushing 1");
            subject.OnNext(1);
            Debug.Log("Pushing 2");
            subject.OnNext(2);
            Debug.Log("Pushing 3");
            subject.OnNext(3);
            Debug.Log("Pushing 4");
            subject.OnNext(4);
            Debug.Log("Pushing 5");
            subject.OnNext(5);
            subject.OnCompleted();
        }
    }
}
