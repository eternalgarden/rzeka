using Rzeka;
using Rzeka.Unirx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Examples.Fillory
{
    /*
     * CombineLatest and Zip do NOT complete when it's single element completes
     * 
     * AndThenWhen completes when it's first item completes. (THIS IS SO CRYPTIC)
     * Does that mean Rx.NET doesnt really care much for completion of streams.
     * 
     * However AndThenWhen behaves like .Zip under the hood without being explicit about it.
     * Which means it doesn't output values unless each of it's items produced a new value.. :(
     * And even then it plays them as queued and not the latest value.
     */
    public class Completion : LoomingMono
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            UpdateExperiments1();
            //CombineLatestCompletion();
            //AndThenCompletion();
            //SubjectAndCompletedObsercavleZip();
        }

        private void Update()
        {
            //Debug.Log("halp");

            if (Input.GetKeyDown(KeyCode.B))
            {
                Debug.Log("halp");
            }
        }

        class Data
        {
            public string Name;
            public int SunSign;
        }

        private void UpdateExperiments1()
        {
            Debug.Log("halp");

            var keyPressStreamA = UnityObservable
                .EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.A))
                .Select(x =>
                {
                    Debug.Log("oik");
                    return $"Key A pressed at {x}";
                });

            var intervalStream = Observable
                .Interval(TimeSpan.FromSeconds(0.5f));

            var importantData = Observable
                .Timer(TimeSpan.FromSeconds(5))
                .Select(_ => new Data { Name = "Maria", SunSign = 4 });

            q += keyPressStreamA
                .CombineLatest(intervalStream, importantData)
                .Subscribe(o =>
                {
                    Debug.Log($"{o.First} :: {o.First} :: {o.Third.Name} is {o.Third.SunSign}");
                });

            q += keyPressStreamA.Subscribe(o =>
            {
                Debug.Log("halp");
            });
        }

        private void CombineLatestCompletion()
        {
            Debug.Log("--- Combine Latest ---");

            Subject<float> subject1 = new Subject<float>();

            Subject<float> subject2 = new Subject<float>();

            q += subject1
                .CombineLatest(subject2)
                .Subscribe(
                   onNext: (x) =>
                   {
                       Debug.Log("onNext: " + x);
                   },
                   onCompleted:
                   () =>
                   {
                       Debug.Log("onCompleted");
                   });

            subject1.OnNext(2f);
            subject2.OnNext(3f);
            subject1.OnCompleted(); // Both need to be complete to fire oncompleted
            subject2.OnCompleted();
        }

        private void AndThenCompletion()
        {
            Debug.Log("--- And Then When ---");


            Subject<float> subject1 = new Subject<float>();

            Subject<float> subject2 = new Subject<float>();

            var andthen = subject1
                .And(subject2)
                .Then((a, v) => new { a, v });

            q += Observable
                .When(andthen)
                .Subscribe(
                   onNext: (x) =>
                   {
                       Debug.Log("onNext: " + x);
                   },
                   onCompleted:
                   () =>
                   {
                       Debug.Log("onCompleted");
                   });

            subject1.OnNext(2f);
            subject2.OnNext(3f);
            subject2.OnNext(6f);
            subject2.OnNext(8f);
            subject1.OnNext(4f);

            // ! OK THIS IS OBSCENELY CRYPTIC
            //subject1.OnCompleted(); // onComplleted is getting called on AndThen stream
            subject2.OnCompleted(); // ! onCompleted is NOT getting called on AndThen streaam

            Subject<(int a, float b)> sss = new();
        }

        void SubjectAndCompletedObsercavleZip()
        {
            var returnNum = Observable.Return(1.0f);

            Subject<float> subject = new Subject<float>();

            q += returnNum
                .Zip(subject)
                .Subscribe(
                   onNext: (x) =>
                   {
                       Debug.Log("onNext: " + x);
                   },
                   onCompleted:
                   () =>
                   {
                       Debug.Log("onCompleted");
                   });

            subject.OnNext(2f);

            // ! it wont complete untill both are complete
            //subject.OnCompleted();
        }
    }
}
