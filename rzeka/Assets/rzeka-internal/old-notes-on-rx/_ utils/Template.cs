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
    public class Template : LoomingMono
    {
        //[Button(".SomeMethod()")]
        void SomeMethod()
        {

        }

        [InfoBox("")]
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
                    //
                    });
            }
        }
    }
}
