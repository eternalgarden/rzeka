using Rzeka;
using Rzeka.Unirx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
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
        IObservable<long> keyPressStreamA;
        IObservable<long> keyPressStreamD;



        protected override void OnEnable()
        {
            base.OnEnable();

            var keyPressStreamA = UnityObservable
                .EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.A));

            var keyPressStreamD = UnityObservable
                .EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.D));

            WeaveExperiments();
            //UpdateExperiments1();
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

        public class UserData : TMatter
        {
            public Guid Guid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Guid[] Circumstances { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public string Description => throw new NotImplementedException();

            public string Name { get; set; }
            public string Zodiac { get; set; }
            public int FavNumber { get; set; }
            public DateTime JoinedDate { get; set; }
        }

        public class UserWelcomingText : TMatter
        {
            public Guid Guid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Guid[] Circumstances { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public string Description => throw new NotImplementedException();

            public string WelcomingText { get; set; }
        }

        public interface IBindingSpell
        {
            Type[] Requirements { get; }
            bool this[Type type] { set; }
        }

        public interface IGivingSpell<T> where T : TMatter
        {
            IObservable<T> Cast(params object[] ingredients);
        }

        public abstract class ScrollBase : IDisposable
        {
            public abstract bool IsCastable { get; }
            public abstract void Dispose();
        }

        public class Scroll<Q> : ScrollBase, IGivingSpell<Q> where Q : TMatter
        {
            public IObservable<Q> spell;

            public override bool IsCastable => true;

            public IObservable<Q> Cast(params object[] ingredients)
            {
                return spell;
            }

            public override void Dispose()
            {
                spell = null;
            }
        }

        public class Scroll<T, Q> : ScrollBase, IBindingSpell, IGivingSpell<Q> where Q : TMatter where T : TMatter
        {
            public Func<IObservable<T>, IObservable<Q>> spell;
            readonly Dictionary<Type, bool> _availableIngredients = new(1)
            {
                { typeof(T), false }
            };
            readonly Type[] _requirements = new[] { typeof(T) };

            public Type[] Requirements => _requirements;
            public bool this[Type type]
            {
                set
                {
                    if (_availableIngredients.ContainsKey(type) is false)
                    {
                        throw new Exception("wrong");
                    }
                    else
                    {
                        _availableIngredients[type] = value;
                    }
                }
            }

            public override bool IsCastable => _availableIngredients.All(kvp => kvp.Value == true);

            public IObservable<Q> Cast(params object[] ingredients)
            {
                return spell.Invoke(ingredients[0] as IObservable<T>);
            }

            public override void Dispose()
            {
                spell = null;
            }
        }

        public class TakerSpell<T> : ScrollBase, IBindingSpell where T : TMatter
        {
            public Func<IObservable<T>, IDisposable> spell;

            public bool this[Type type] => type == typeof(T);

            public override void Dispose()
            {
                spell = null;
            }
        }

        Dictionary<Type, object> _allSpells = new();
        Dictionary<Type, object> _castableSpells = new(); // ! to contain IGivingSpell<T>

        Dictionary<Type, List<object>> _spellsWaitingForIngredientOfType = new();

        //Dictionary<Type, object> _giverSpells = new();
        Dictionary<Type, List<object>> _takerSpells = new();
        void DeactivateSpell(ScrollBase spell)
        {

        }

        IObservable<UserData> providerDD; // ! if was cold, providerS should be disconnected aswell, if it's type would keep last value it should be kept and not disconnected
        IObservable<string> providerS;

        IDisposable MakeSpell(IObservable<UserData> spell)
        {
            Scroll<UserData> Spell = new() { spell = spell };

            _allSpells.Add(typeof(UserData), Spell);

            _castableSpells.Add(typeof(UserData), Spell);

            return Disposable.Create(() => DeactivateSpell(Spell));
        }

        IDisposable MakeSpell(Func<IObservable<UserData>, IObservable<UserWelcomingText>> spell)
        {
            Scroll<UserData, UserWelcomingText> Spell = new() { spell = spell };

            _allSpells.Add(typeof(UserWelcomingText), Spell);

            if (_castableSpells.ContainsKey(typeof(UserData)))
            {
                Spell[typeof(UserData)] = true;

                // ! so in most cases (?) you won't be able to stop a spell that has already been fully cast
                // ! however they will be only cast on specific demand, otherwise they will be kept as Scrolls
                IGivingSpell<UserData> ingredtientSpell = _castableSpells[typeof(UserData)] as IGivingSpell<UserData>;
                IObservable<UserData> ingredient = ingredtientSpell.Cast();

            }
            else
            {
                // TODO waitlist
                _spellsWaitingForIngredientOfType[typeof(UserData)].Add(spell);
            }

            IObservable<UserWelcomingText> newGiver = Spell.Cast(ingredient);

            return Disposable.Create(() => providerS = null);
        }

        IDisposable MakeSpell(Func<IObservable<UserWelcomingText>, IDisposable> spell)
        {
            TakerSpell<UserWelcomingText> Spell = new() { spell = spell };

            //_takerSpells.Add(typeof(string), Spell);

            return Disposable.Create(() => providerS = null);
        }

        void WeaveExperiments()
        {
            IDisposable userData = MakeSpell(Observable
                .Return(new UserData { Name = "Maria", Zodiac = "Cancer", FavNumber = 7, JoinedDate = new DateTime(1992, 7, 3) }));

            IDisposable userWelcoming = MakeSpell(spell: userData =>
            {
                return userData
                    .Select(dd => new UserWelcomingText { WelcomingText = $"Hi Maria! Ur a {dd.Zodiac} who joined us {(DateTime.Now - dd.JoinedDate).TotalDays} days ago." });
            });

            IDisposable welcomingPrinter = MakeSpell(welcomingText =>
            {
                return welcomingText.Subscribe(welcoming => Debug.Log(welcoming.WelcomingText));
            });

            q += userData;
            q += userWelcoming;
            q += welcomingPrinter;
        }

        class Data
        {
            public string Name;
            public int SunSign;
        }

        private void UpdateExperiments1()
        {

            var intervalStream = Observable
                .Interval(TimeSpan.FromSeconds(0.5f));

            var importantData = Observable
                .Return(new Data { Name = "Maria", SunSign = 4 })
                .PublishLast();

            q += importantData.Connect();

            q += keyPressStreamA
                .CombineLatest(intervalStream, importantData)
                .Subscribe(o =>
                {
                    Debug.Log($"{o.First} :: {o.Second} :: {o.Third.Name} is {o.Third.SunSign}");
                });

            q += keyPressStreamA
                .Subscribe(o =>
                {
                    Debug.Log($"{o}");
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
