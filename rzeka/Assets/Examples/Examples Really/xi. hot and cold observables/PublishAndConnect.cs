/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka.Examples
{
    /* 🌊 ---- ---- */

    public class PublishAndConnect : LoomingMono
    {
        /*

        http://introtorx.com/Content/v1.0.10621.0/14_HotAndColdObservables.html#PublishAndConnect
        https://reactivex.io/documentation/operators/publish.html

        .Publish() operator transforms IObservable into an IConnectableObservable

        IConnectableObservable adds one method: .Connect()

        "A connectable Observable resembles an ordinary Observable, except that it does not begin 
        emitting items when it is subscribed to, but only when the Connect operator is applied to 
        it. In this way you can prompt an Observable to begin emitting items at a time of your 
        choosing."

        */

        IConnectableObservable<long> _tick;

        IDisposable _connect;
        IDisposable _coolLog;

        #region Playmode Example

        protected override void OnEnable()
        {
            // -------------

            base.OnEnable();

            TimeSpan second = TimeSpan.FromSeconds(1);

            // Publish doesnt wait for subscription to start emitting it's items
            // It waits for Connect
            _tick = Observable
                .Interval(second)
                .Publish();

            // Notice nothing happens, still waiting for .Connect()
            q += _tick.Subscribe(t =>
            {
                Debug.Log(t);
            });

            // -------------
        }

        void Update()
        {
            // -------------

            // With this key you can toggle connection
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (_connect is null)
                {
                    q += _connect = _tick.Connect();
                }
                else
                {
                    _connect.Dispose();
                    _connect = null;
                }
            }

            // With F you can add additional subscriber
            // Notice that if you havent pressed A yet it will still not output anything
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (_coolLog is null)
                {
                    q += _coolLog = _tick.Subscribe(t =>
                    {
                        Debug.Log($"<color=yellow>Ima: {t}</color>");
                    });
                }
                else
                {
                    _coolLog.Dispose();
                    _coolLog = null;
                }
            }

            // -------------
        }

        #endregion // Playmode Example

        /* ---- ---- ⛺ */
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 22 July 2022 🌊 */