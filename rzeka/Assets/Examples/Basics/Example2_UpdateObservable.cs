using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using UnityEngine;
using RzekaRiver;

public class Example2_UpdateObservable : MonoBehaviour
{
    [SerializeField] bool spamRegularUpdate;

    IObservable<long> _observableUpdate;
    CompositeDisposable _subscriptions;

    void Awake()
    {
        // -------------
        
        _observableUpdate = UnityObservable.EveryUpdate();
        _subscriptions = new CompositeDisposable();

        // -------------
    }

    void Update()
    {
        // -------------

        if (spamRegularUpdate)
        {
            Debug.Log($"<color=white>Regular Update</color>");
        }

        // -------------
    }

    // * https://docs.unity3d.com/Manual/ExecutionOrder.html
    void OnEnable()
    {
        // -------------

        _subscriptions.Add(_observableUpdate
            .Subscribe(
                onNext: _ =>
                {
                    Debug.Log($"<color=yellow>Observable Update</color>");
                }
        ));

        _subscriptions.Add(_observableUpdate
            .Where(frame => frame % 5 == 0) // every fifth frame
            .Select(frame => $"Well this was a 5-divisible frame (${frame}) from the relativistic perspective of this observable, frame.")
            .Subscribe(
                onNext: logText =>
                {
                    Debug.Log($"<color=cyan>{logText}</color>");
                }
        ));

        // -------------
    }

    void OnDisable()
    {
        // -------------

        _subscriptions.Clear();

        // -------------
    }
}
