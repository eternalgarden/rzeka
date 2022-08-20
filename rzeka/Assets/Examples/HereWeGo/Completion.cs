using Rzeka;
using Rzeka.Unirx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Linq;
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

            keyPressStreamA = UnityObservable
                .EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.A));

            keyPressStreamD = UnityObservable
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

        public interface TScrollBase : IDisposable
        {
            Guid Guid { get; }
            bool IsCastable { get; }
        }

        public interface TBindingScroll : TScrollBase
        {
            Dictionary<Type, bool> AvailableIngredientsDictionary { get; }
            Type[] Requirements { get; }
            public bool this[Type type]
            {
                get
                {
                    if (AvailableIngredientsDictionary.ContainsKey(type) is false) throw new Exception("Unexpected requested type. Did you check requirements first.");
                    else return AvailableIngredientsDictionary[type];
                }
                set
                {
                    if (AvailableIngredientsDictionary.ContainsKey(type) is false) return;

                    AvailableIngredientsDictionary[type] = value;
                }
            }

            bool TScrollBase.IsCastable => AvailableIngredientsDictionary.All(kvp => kvp.Value == true);
        }

        public interface TAlteringScroll : TBindingScroll
        {
            // todo remove try throw errr instgead
            void Cast(Completion library);
        }

        public interface IConjuringScroll : TScrollBase
        {
            Type ConjuredType { get; }
            // todo remove try throw errr instgead
            bool TryCast(out object observableSpell, Completion library);
        }

        public interface TConjuringScroll<T> : IConjuringScroll where T : TMatter
        {
            bool TryCast(out IObservable<T> observableSpell, Completion library);
        }

        public class Scroll<Q> : TScrollBase, TConjuringScroll<Q> where Q : TMatter
        {
            public IObservable<Q> spell;

            Guid _guid = new();
            public Guid Guid => _guid;

            public bool IsCastable => true;

            public Type ConjuredType => typeof(Q);

            public bool TryCast(out IObservable<Q> givingSpell, Completion library)
            {
                givingSpell = spell;
                return true;
            }

            public bool TryCast(out object observableSpell, Completion library)
            {
                observableSpell = spell;
                return true;
            }

            public void Dispose()
            {
                spell = null;
            }
        }

        public class Scroll<T, Q> : TScrollBase, TBindingScroll, TConjuringScroll<Q> where Q : TMatter where T : TMatter
        {
            public Func<IObservable<T>, IObservable<Q>> spell;

            public Type[] Requirements { get; } = new[] { typeof(T) };

            public Dictionary<Type, bool> AvailableIngredientsDictionary { get; } = new(1)
            {
                { typeof(T), false }
            };

            public Type ConjuredType => typeof(Q);

            Guid _guid = new();
            public Guid Guid => _guid;

            public bool TryCast(out object observableSpell, Completion library)
            {
                bool success = TryCast(out IObservable<Q> spell, library);
                observableSpell = spell;
                return success;
            }

            public bool TryCast(out IObservable<Q> observableSpell, Completion library)
            {
                observableSpell = null;

                if ((this as TBindingScroll).IsCastable)
                {
                    if (library.AskForIngredient<T>(out IObservable<T> ingredtient))
                    {
                        observableSpell = spell.Invoke(ingredtient);
                    }
                    else
                    {
                        throw new Exception("messed up");
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Dispose()
            {
                spell = null;
            }
        }

        public class AlteringScroll<T> : TScrollBase, TAlteringScroll where T : TMatter
        {
            public Action<IObservable<T>> spell;

            Guid _guid = new();
            public Guid Guid => _guid;

            public Type[] Requirements { get; } = new[] { typeof(T) };

            public Dictionary<Type, bool> AvailableIngredientsDictionary { get; } = new(1)
            {
                { typeof(T), false }
            };

            public void Dispose()
            {
                spell = null;
            }

            public void Cast(Completion library)
            {
                if ((this as TBindingScroll).IsCastable)
                {
                    if (library.AskForIngredient<T>(out IObservable<T> ingredtient))
                    {
                        spell.Invoke(ingredtient);
                    }
                    else
                    {
                        throw new Exception("messed up");
                    }
                }
                else
                {
                    throw new Exception("Tried to cast an uncastable spell. Ingredients check must have failed.");
                }
            }
        }

        // todo possibly rename to castableConjurings
        Dictionary<Type, List<IConjuringScroll>> _castableConjuringScrolls = new(); // ! to contain IGivingSpell<T><T>
        Dictionary<Type, List<TBindingScroll>> _blockedScrollsByRequiredType = new();

        IObservable<UserData> providerDD; // ! if was cold, providerS should be disconnected aswell, if it's type would keep last value it should be kept and not disconnected
        IObservable<string> providerS;

        void AddACastableScroll<T>(TConjuringScroll<T> scroll) where T : TMatter
        {
            AddACastableScroll(typeof(T), scroll);
        }

        void AddACastableScroll(Type castableScrollType, IConjuringScroll scroll)
        {
            if (_castableConjuringScrolls.ContainsKey(castableScrollType) is false)
            {
                _castableConjuringScrolls[castableScrollType] = new List<IConjuringScroll>();
            }

            _castableConjuringScrolls[castableScrollType].Add(scroll);

            // todo check potential waiters in _spellsWaitingForIngredientOfType

            if (_blockedScrollsByRequiredType.ContainsKey(castableScrollType))
            {
                //_blockedScrollsByRequiredType[type]
                //    .ToObservable()
                //    .Do(bindingScroll => bindingScroll[type] = true)
                //    .Where(bindingScroll => bindingScroll.IsCastable)
                // !  .Do(bindingScroll => ) // ! data in domain

                foreach (TBindingScroll blockedScroll in _blockedScrollsByRequiredType[castableScrollType])
                {
                    blockedScroll[castableScrollType] = true;

                    if (blockedScroll.IsCastable)
                    {
                        if (blockedScroll is IConjuringScroll conjuringScroll)
                        {
                            // ! recurse
                            AddACastableScroll(conjuringScroll.ConjuredType, conjuringScroll);
                        }
                        else if (blockedScroll is TAlteringScroll alteringScroll)
                        {
                            // ! just cast it
                            alteringScroll.Cast(this);
                        }
                    }
                }
            }
        }

        void AddABlockedScroll(TBindingScroll scroll)
        {
            foreach (Type req in scroll.Requirements)
            {
                if (scroll[req] == false)
                {
                    if (_blockedScrollsByRequiredType.ContainsKey(req) is false)
                    {
                        _blockedScrollsByRequiredType[req] = new List<TBindingScroll>();
                    }

                    _blockedScrollsByRequiredType[req].Add(scroll);
                }
            }
        }

        void RemoveABlockedScroll(TBindingScroll scroll)
        {
            foreach (Type req in scroll.Requirements)
            {
                if (scroll[req] == false)
                {
                    _blockedScrollsByRequiredType[req]
                        .First();

                    if (_blockedScrollsByRequiredType[req].Count == 0)
                    {
                        _blockedScrollsByRequiredType.Remove(req);
                    }
                }
            }
        }

        void RemoveAKnownScroll<T>(TScrollBase scroll)
        {
            Type removedSpellType = typeof(T);

            if (scroll.IsCastable && scroll is IConjuringScroll conjuringScroll)
            {
                RemoveFromConjuringScrolls(removedSpellType, conjuringScroll);
            }
            else
            {
                if (scroll is TBindingScroll bindingScroll)
                {
                    foreach (Type req in bindingScroll.Requirements)
                    {
                        if (bindingScroll[req] == false)
                        {
                            if (_blockedScrollsByRequiredType[req].Contains(bindingScroll) is false) throw new Exception("ummmm");
                            _blockedScrollsByRequiredType[req].Remove(bindingScroll);
                        }
                    }
                }
            }
        }

        void RemoveFromConjuringScrolls(Type removedSpellType, IConjuringScroll scroll)
        {
            //if (_castableConjuringScrolls[removedSpellType].Contains(scroll) is false) throw new Exception("this is bad");
            if (_castableConjuringScrolls[removedSpellType].RemoveAll(x => x.Guid == scroll.Guid) == 0) throw new Exception("unexpected bewware");

            if (_castableConjuringScrolls[removedSpellType].Count == 0)
            {
                _castableConjuringScrolls.Remove(removedSpellType);
            }

            List<(Type key, TBindingScroll scroll)> newBlockedScrolls = new();

            var thing = _castableConjuringScrolls
                .ToObservable()
                .Where(kvp => kvp.Value is TBindingScroll)
                .Select(kvp => (key: kvp.Key, spell: kvp.Value as TBindingScroll))
                .Where(o => o.spell.Requirements.Contains(removedSpellType))
                .Subscribe(o =>
                {
                    //throw new Exception("Test");
                    o.spell[removedSpellType] = false;
                    newBlockedScrolls.Add((o.key, o.spell));
                });

            foreach (var newBlockedScroll in newBlockedScrolls)
            {
                // TODO now add a check for existing blocked scrolls
                AddABlockedScroll(newBlockedScroll.scroll);

                if (newBlockedScroll.scroll is IConjuringScroll conjuringScroll)
                {
                    RemoveFromConjuringScrolls(newBlockedScroll.key, conjuringScroll); // ! recurse
                }
            }
        }

        bool AskForIngredient<T>(out IObservable<T> ingredient) where T : TMatter
        {
            ingredient = null;
            Type type = typeof(T);

            if (_castableConjuringScrolls.ContainsKey(type))
            {
                List<IConjuringScroll> scrolls = _castableConjuringScrolls[type];

                // todo handling multiple providers
                if (scrolls.Count > 1) throw new NotImplementedException("multiple castable scrolls of same type");

                var conjuringScroll = scrolls[0] as TConjuringScroll<T>;

                if (conjuringScroll.TryCast(out IObservable<T> givingSpell, this))
                {
                    ingredient = givingSpell;
                    return true;
                }
                else return false;
                //foreach (var scroll in scrolls)
                //{

                //}
            }
            else return false;
        }

        IDisposable Pluck<T>(IObservable<T> spell) where T : TMatter
        {
            Scroll<T> Scroll = new() { spell = spell };

            Type type = typeof(T);

            AddACastableScroll(type, Scroll);

            return Disposable.Create(() => RemoveFromConjuringScrolls(type, Scroll));
        }

        IDisposable Loom<T, Q>(Func<IObservable<T>, IObservable<Q>> spell) where T : TMatter where Q : TMatter
        {
            Scroll<T, Q> Scroll = new() { spell = spell };

            TBindingScroll bindingSpell = Scroll as TBindingScroll;
            foreach (var req in Scroll.Requirements)
            {
                if (_castableConjuringScrolls.ContainsKey(req))
                {
                    (bindingSpell)[req] = true;
                }
            }

            if (bindingSpell.IsCastable)
            {
                AddACastableScroll(Scroll);
            }
            else
            {
                // ! so in most cases (?) you won't be able to stop a spell that has already been fully cast
                // ! however they will be only cast on specific demand, otherwise they will be kept as Scrolls
                // todo waitlist
                AddABlockedScroll(Scroll);

            }

            return Disposable.Create(() => RemoveAKnownScroll<Q>(Scroll));
        }

        IDisposable Weave<T>(Action<IObservable<T>> spell) where T : TMatter
        {
            // todo solve the missing concept of adding altering spells to _allKnownSpells as they cannot accept those at the moment
            // todo otherwise consider renaming or altogether burning down the all known spells library
            Type type = typeof(T);
            AlteringScroll<T> Scroll = new() { spell = spell };

            foreach (Type req in Scroll.Requirements)
            {
                if (_castableConjuringScrolls.ContainsKey(req))
                {
                    (Scroll as TBindingScroll)[req] = true;
                }
            }

            if ((Scroll as TBindingScroll).IsCastable)
            {
                Scroll.Cast(this);
                return Disposable.Empty;
            }
            else
            {
                Debug.LogError("Failed Cast Weave!");
                AddABlockedScroll(Scroll);
                return Disposable.Create(() => RemoveABlockedScroll(Scroll));
            }
        }

        void WeaveExperiments()
        {
            IDisposable userData = Pluck<UserData>(Observable
                .Return(new UserData { Name = "Maria", Zodiac = "Cancer", FavNumber = 7, JoinedDate = new DateTime(1992, 7, 3) }));

            IDisposable userWelcoming = Loom<UserData, UserWelcomingText>(userData =>
            {
                return userData
                    .Select(dd => new UserWelcomingText { WelcomingText = $"Hi Maria! Ur a {dd.Zodiac} who joined us {(int)(DateTime.Now - dd.JoinedDate).TotalDays} days ago." });
            });

            IDisposable welcomingPrinter = Weave<UserWelcomingText>(welcomingText =>
            {
                welcomingText.Subscribe(welcoming => Debug.Log(welcoming.WelcomingText));
            });

            userWelcoming.Dispose();

            using var anotherPrinter = Weave<UserWelcomingText>(welcomingText =>
            {
                welcomingText.Subscribe(welcoming => Debug.Log($"<color=yellow>{welcoming.WelcomingText}</color>"));
            });

            q += keyPressStreamA
                .Subscribe(o =>
                {
                    using var anotherWelcoming = Loom<UserData, UserWelcomingText>(userData =>
                    {
                        return userData
                            .Select(dd => new UserWelcomingText { WelcomingText = $"Hi Maria! Ur a {dd.Zodiac} who joined us {(DateTime.Now - dd.JoinedDate).TotalDays} days ago." });
                    });
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
