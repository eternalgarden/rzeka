using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Joins;
using System.Reactive;
using UnityEngine;
using System;
using System.Linq;
using Rzeka.Unirx;
using System.Reactive.Subjects;
using System.Reactive.Disposables;

namespace SpellsAndData
{
    public struct RaycastRequest : IRequest
    {
        public string Description => "performing a raycast";
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public Vector3 Origin { get; set; }
        public Vector3 Direction { get; set; }

        public RaycastRequest(Vector3 origin, Vector3 direction, params TMatter[] circumstances)
        {
            Guid = Guid.NewGuid();
            Circumstances = circumstances.Select(x => x.Guid).ToArray();
            Origin = origin;
            Direction = direction;
        }
        
        public class Result : IResponse<RaycastRequest>
        {
            public string Description => "result of: " + Request.Description;
            public Guid Guid { get; set; }
            public Guid[] Circumstances { get; set; }
            public RaycastRequest Request { get; set; }
            
            public bool Hit { get; set; }
            public Vector3 Point { get; set; }
            public RaycastHit RaycastHit { get; set; }

            public void SetValues(RaycastRequest request, bool hit, Vector3 point, RaycastHit raycastHit)
            {
                Request = request;
                Hit = hit;
                Point = point;
                RaycastHit = raycastHit;
            }
        }
    }

    public class SADExperiments : LoomingMono
    {
        IObservable<long> _timer;
        // Start is called before the first frame update
        protected override void OnEnable()
        {
            base.OnEnable();

            TestSubjectWrapping();

        }

        void NullObservableTest()
        {
            _timer = Observable
                .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1));

            q += UnityObservable
                .EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Return))
                .Subscribe(_ =>
                {
                    _timer = null;
                });


            q += Observable
                .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                .Merge(_timer)
                .Subscribe(x =>
                {
                    Debug.Log(x.ToString());
                });
        }

        class MultipleSourceTester : TMatter, TAllowingMultipleSources<MultipleSourceTester>
        {
            public Guid Guid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Guid[] Circumstances { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public string Description => throw new NotImplementedException();

            public string data;
        }

        void TestSubjectWrapping()
        {
            var sourceOneObservable = Observable
                .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                .Select(x =>
                {
                    return new MultipleSourceTester()
                    {
                        data = $"SOURCE <<1>> : {x}"
                    };
                });

            var originalSubject = new Subject<MultipleSourceTester>(); //            subject = Subjects[keyOut] as SubjectBase<Tout>;

            // ! subject already signed in to some observable
            // those can stay, thats not a problem
            IDisposable subscription = sourceOneObservable.Subscribe(originalSubject);
            q += subscription;

            // ! someone alrady signed in to that subject
            // TODO the observers have to be moved somehow though to the merged stream
            var observer1 = Observer.Create<MultipleSourceTester>(onNext: val =>
            {
                Debug.Log($"<color=yellow>{val.data}</color>");
            });

            q += originalSubject.Subscribe(observer1);

            var sourceANOTHERObservable = Observable
                .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(2))
                .Select(x =>
                {
                    return new MultipleSourceTester()
                    {
                        data = $"ANOTHER <<1>> : {x}"
                    };
                });

            //// TODO this would be too easy, must verify
            //// TODO simple wrapping of previous subject with its observers
            var combinedObservable = (new MultipleSourceTester() as TAllowingMultipleSources<MultipleSourceTester>)
                .CombineSourcesPattern(originalSubject.AsObservable(), sourceANOTHERObservable);

            var newSubject = new Subject<MultipleSourceTester>();

            //q += combinedObservable.Subscribe(newSubject);
            //q += newSubject.Subscribe(observer1);



            //originalSubject = Subject.Create<MultipleSourceTester>(
            //    observer: originalSubject,
            //    observable: combinedObservable) as Subject<MultipleSourceTester>;

            //Subjects[keyOut] = subject;
        }
    }
}
