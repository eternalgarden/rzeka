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

        public interface IScrollBase : IDisposable
        {
            bool IsCastable { get; }
        }

        public interface TBindingScroll : IScrollBase
        {
            Dictionary<Type, bool> AvailableIngredientsDictionary { get; }
            Type[] Requirements { get; }
            public bool this[Type type]
            {
                set
                {
                    if (AvailableIngredientsDictionary.ContainsKey(type) is false) return;

                    AvailableIngredientsDictionary[type] = value;
                }
            }

            bool IScrollBase.IsCastable => AvailableIngredientsDictionary.All(kvp => kvp.Value == true);
        }

        public interface IConjuringScroll<T> where T : TMatter
        {
            bool TryCast(out IObservable<T> givingSpell, Completion library);
        }

        public interface IAlteringScroll : TBindingScroll
        {
            bool TryCast(out IDisposable disposable, Completion library);
        }

        public class Scroll<Q> : IScrollBase, IConjuringScroll<Q> where Q : TMatter
        {
            public IObservable<Q> spell;

            public bool IsCastable => true;

            public bool TryCast(out IObservable<Q> givingSpell, Completion library)
            {
                givingSpell = spell;
                return true;
            }

            public void Dispose()
            {
                spell = null;
            }
        }

        public class Scroll<T, Q> : IScrollBase, TBindingScroll, IConjuringScroll<Q> where Q : TMatter where T : TMatter
        {
            public Func<IObservable<T>, IObservable<Q>> spell;

            public Type[] Requirements { get; } = new[] { typeof(T) };

            public Dictionary<Type, bool> AvailableIngredientsDictionary { get; } = new(1)
            {
                { typeof(T), false }
            };

            public bool TryCast(out IObservable<Q> givingSpell, Completion library)
            {
                givingSpell = null;

                if ((this as TBindingScroll).IsCastable)
                {
                    if (library.AskForIngredient<T>(out IObservable<T> ingredtient))
                    {
                        givingSpell = spell.Invoke(ingredtient);
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

        public class AlteringScroll<T> : IScrollBase, IAlteringScroll where T : TMatter
        {
            public Func<IObservable<T>, IDisposable> spell;

            public Type[] Requirements { get; } = new[] { typeof(T) };

            public Dictionary<Type, bool> AvailableIngredientsDictionary { get; } = new(1)
            {
                { typeof(T), false }
            };

            public void Dispose()
            {
                spell = null;
            }

            public bool TryCast(out IDisposable disposable, Completion library)
            {
                disposable = null;

                if ((this as TBindingScroll).IsCastable)
                {
                    if (library.AskForIngredient<T>(out IObservable<T> ingredtient))
                    {
                        disposable = spell.Invoke(ingredtient);
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
        }

        Dictionary<Type, object> _allKnownScrolls = new();
        Dictionary<Type, object> _castableScrolls = new(); // ! to contain IGivingSpell<T><T>
        Dictionary<Type, List<TBindingScroll>> _blockedScrolls = new();

        //Dictionary<Type, object> _giverSpells = new();
        Dictionary<Type, List<object>> _takerSpells = new();

        IObservable<UserData> providerDD; // ! if was cold, providerS should be disconnected aswell, if it's type would keep last value it should be kept and not disconnected
        IObservable<string> providerS;


        void AddAKnownScroll<T>(IScrollBase spell)
        {
            Type type = typeof(T);
            _allKnownScrolls.Add(type, spell);
        }

        void AddACastableScroll<T>(IConjuringScroll<T> scroll) where T : TMatter
        {
            Type type = typeof(T);
            _castableScrolls.Add(type, scroll);

            // todo check potential waiters in _spellsWaitingForIngredientOfType
        }

        void AddABlockedScroll(Type type, TBindingScroll scroll)
        {
            if (_blockedScrolls.ContainsKey(type) is false)
            {
                _blockedScrolls[type] = new List<TBindingScroll>();
            }

            _blockedScrolls[type].Add(scroll);
        }

        void RemoveAKnownScroll<T>(IScrollBase scroll)
        {
            Type removedSpellType = typeof(T);

            _allKnownScrolls.Remove(removedSpellType); // todo handling multiple of same type///

            if (_castableScrolls.ContainsKey(removedSpellType))
            {
                RemoveFromCastableScrolls(removedSpellType, scroll);
            }
        }

        void RemoveFromCastableScrolls(Type removedSpellType, IScrollBase scroll)
        {
            _castableScrolls.Remove(removedSpellType);

            List<(Type key, TBindingScroll scroll)> newBlockedScrolls = new();

            var thing = _castableScrolls
                .ToObservable()
                .Where(kvp => kvp.Value is TBindingScroll)
                .Select(kvp => (key: kvp.Key, spell: kvp.Value as TBindingScroll))
                .Where(o => o.spell.Requirements.Contains(removedSpellType))
                .Subscribe(o =>
                {
                    o.spell[removedSpellType] = false;
                    newBlockedScrolls.Add((o.key, o.spell));
                });

            foreach (var newBlockedScroll in newBlockedScrolls)
            {
                // TODO now add a check for existing blocked scrolls
                AddABlockedScroll(newBlockedScroll.key, newBlockedScroll.scroll);
                RemoveFromCastableScrolls(newBlockedScroll.key, newBlockedScroll.scroll); // ! recurse
            }
        }

        bool AskForIngredient<T>(out IObservable<T> ingredient) where T : TMatter
        {
            ingredient = null;
            Type type = typeof(T);

            if (_castableScrolls.ContainsKey(type))
            {
                var scroll = _castableScrolls[type] as IConjuringScroll<T>;

                if (scroll.TryCast(out IObservable<T> givingSpell, this))
                {
                    ingredient = givingSpell;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        IDisposable Pluck<T>(IObservable<T> spell) where T : TMatter
        {
            Scroll<T> Spell = new() { spell = spell };

            Type type = typeof(T);
            AddAKnownScroll<T>(Spell);
            AddACastableScroll(Spell);

            return Disposable.Create(() => RemoveAKnownScroll<T>(Spell));
        }

        IDisposable Loom<T, Q>(Func<IObservable<T>, IObservable<Q>> spell) where T : TMatter where Q : TMatter
        {
            Scroll<T, Q> Scroll = new() { spell = spell };

            AddAKnownScroll<Q>(Scroll); // TODO this will require handling multiple providers of same type of spell

            TBindingScroll bindingSpell = Scroll as TBindingScroll;
            foreach (var req in Scroll.Requirements)
            {
                if (_castableScrolls.ContainsKey(req))
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
            }

            return Disposable.Create(() => RemoveAKnownScroll<T>(Scroll));
        }

        IDisposable Weave<T>(Func<IObservable<T>, IDisposable> spell) where T : TMatter
        {
            AlteringScroll<T> Spell = new() { spell = spell };

            foreach (var req in Spell.Requirements)
            {
                if (_castableScrolls.ContainsKey(req))
                {
                    (Spell as TBindingScroll)[req] = true;
                }
            }

            if (Spell.TryCast(out IDisposable weaving, this))
            {
                Debug.Log("Yas Cast Weave!");
                return weaving;
            }
            else
            {
                Debug.LogError("Failed Cast Weave!");
                return Disposable.Empty;
            }
        }

        void WeaveExperiments()
        {
            IDisposable userData = Pluck<UserData>(Observable
                .Return(new UserData { Name = "Maria", Zodiac = "Cancer", FavNumber = 7, JoinedDate = new DateTime(1992, 7, 3) }));

            IDisposable userWelcoming = Loom<UserData, UserWelcomingText>(userData =>
            {
                return userData
                    .Select(dd => new UserWelcomingText { WelcomingText = $"Hi Maria! Ur a {dd.Zodiac} who joined us {(DateTime.Now - dd.JoinedDate).TotalDays} days ago." });
            });

            IDisposable welcomingPrinter = Weave<UserWelcomingText>(welcomingText =>
            {
                return welcomingText.Subscribe(welcoming => Debug.Log(welcoming.WelcomingText));
            });

            userWelcoming.Dispose();

            using var _ = Weave<UserWelcomingText>(welcomingText =>
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
