using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;
using System.Threading;
using Rzeka.Unirx;
using System.Reactive;

namespace Rzeka.Examples
{
    public class Materialize : LoomingMono
    {
        [InfoBox("Honmestly Materialize doesnt seem to make much sense for me, what's it's use when you can simply just subscribe to this observable?", EInfoBoxType.Warning)]
        [SerializeField] bool RunPlaymodeExample = true;

        protected override void OnEnable()
        {
            if (RunPlaymodeExample is false) return;
            else
            {
                base.OnEnable();

                var returnO = UnityObservable
                    .EveryUpdate()
                    .Where(_ => Input.GetKeyDown(KeyCode.Return));

                var kO = UnityObservable
                    .EveryUpdate()
                    .Where(_ => Input.GetKeyDown(KeyCode.K));

                var xO = UnityObservable
                    .EveryUpdate()
                    .Where(_ => Input.GetKeyDown(KeyCode.X))
                    .Select(_ => new Exception("X Xception"));

                var zip = returnO.And(kO).And(xO)
                    .Then((r, k, x) => new { r, k, x });

                q += Observable
                    .When(zip)
                    .Materialize()
                    .Subscribe(nComb =>
                    {
                        /*
                              public enum NotificationKind
                              {
                              OnNext,
                              OnError,
                              OnCompleted,
                              }
                           */
                        Debug.Log($"New notification of kind: {nComb.Kind} and value: {nComb.Value}");

                        nComb.Accept(
                            onNext: comb => Debug.Log($"Next! comb: {comb}"),
                            onError: e => Debug.Log($"Exception! Ex: {e.Message}"),
                            onCompleted: () => Debug.Log("Its done!"));
                    });
            }
        }
    }
}
