using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rzeka.Unirx
{
    public static partial class UnityObservable
    {
        public static IObservable<bool> FromToggleValueChange(Toggle toggle, out IDisposable token)
        {
            ReplaySubject<bool> subj = new(1);
            
            void Call(bool value) => subj.OnNext(value);

            UnityAction<bool> unityAction = Call;

            toggle.onValueChanged.AddListener(unityAction);

            token = Disposable.Create(() =>
            {
                subj.OnCompleted();
                subj.Dispose();
                toggle.onValueChanged.RemoveListener(unityAction);
            });
            
            return subj.AsObservable();
        }
        
        public static IObservable<uint> FromButtonClick(Button button, out IDisposable token)
        {
            uint count = 0;
            
            ReplaySubject<uint> subj = new(1);
            
            void Call() => subj.OnNext(++count);

            UnityAction unityAction = Call;

            button.onClick.AddListener(unityAction);

            token = Disposable.Create(() =>
            {
                subj.OnCompleted();
                subj.Dispose();
                button.onClick.RemoveListener(unityAction);
            });
            
            return subj.AsObservable();
        }
    }
}
