using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    public class NoManaException : Exception
    {
    }

    [Serializable]
    public class ConjuringScroll<Q> : TConjuringScroll<Q> where Q : TMatter
    {
        readonly IObservable<Q> spell;
        readonly TheLibrary library;
        readonly Eris eris;
        readonly object who;
        readonly Guid _guid = TScrollBase.CreateNewGuid();

        public ConjuringScroll(object who, IObservable<Q> spell, TheLibrary library, Eris debugger)
        {
            this.who = who;
            this.spell = spell;
            this.library = library;
            this.eris = debugger;
        }

        public Guid Guid => _guid;
        public string Title => $"Conjuring of {typeof(Q).Name}";
        public Type ConjuredType => typeof(Q);
        public bool IsCastable => true;
        public object Who => who;

        void Cast(out IObservable<Q> givingSpell)
        {
            givingSpell = spell
                .Materialize()
                .Do(notification => { notification.Accept(eris.GetReleasesObserver<Q>(this)); })
                .Dematerialize();
        }

        public bool TryGetConjuring(out IObservable<Q> observableSpell)
        {
            Cast(out observableSpell);
            return true;
        }

        public void Dispose()
        {
            eris.ScrollWillBeDisposed(this, isNew: false);
            library.RemoveFromConjuringScrolls(this);
        }
    }

    [Serializable]
    public class LoomingScroll<T, Q> : ILoomingScroll<Q> where Q : TMatter where T : TMatter
    {
        readonly Func<IObservable<T>, IObservable<Q>> spell;
        readonly TheLibrary library;
        readonly Eris eris;
        readonly object who;

        public LoomingScroll(object who, Func<IObservable<T>, IObservable<Q>> spell, TheLibrary library, Eris eris)
        {
            this.who = who;
            this.spell = spell;
            this.library = library;
            this.eris = eris;
        }

        public Guid Guid { get; } = TScrollBase.CreateNewGuid();
        public string Title => $"Looming of {typeof(Q).Name} from {(this as TBindingScroll).GetBindingScrollCode()}";
        public Type ConjuredType => typeof(Q);
        public bool IsCastable => (this as TBindingScroll).AreAllIngredientsProvided;
        public bool WasCast => ObservableSpell is not null;
        public Type[] Requirements { get; } = { typeof(T) };
        public BehaviorSubject<bool> HasMana { get; } = new(false);

        public Dictionary<Type, bool> AvailableIngredientsDictionary { get; } = new(1)
        {
            { typeof(T), false }
        };

        public object Who => who;

        IDisposable _noManaObserverContract;
        IObservable<Q> _observableSpell;
        
        
        // TODO WRITE TESTS FOR THIS
        IObservable<Q> ObservableSpell
        {
            get => _observableSpell;
            set
            {
                if (_observableSpell is not null) throw new Exception("Was already cast");

                _observableSpell = value;
                _noManaObserverContract = value
                    .Subscribe(
                        onNext: _ => { },
                        onError: error =>
                        {
                            if (error is NoManaException)
                            {
                                _observableSpell = null;
                            }
                            else
                            {
                                Debug.LogError(error);
                            }

                            _noManaObserverContract.Dispose();
                        });
            }
        }

        // LOOMING SCROLL IS BASICALLY ONLY CAST FROM THE LIBRARY AS AskForIngredient 
        // WHEN IT WAS BLOCKED
        // IF IT WASNT BLOCKED IT WILL BE CAST IMMEDIATELY
        // TODO WHAT IF IT ISNT USED BY ANY WEAVING, WHY WOULD IT BE CAST THEN
        // TODO IT NEEDS CLEANING 
        void Cast(out IObservable<Q> observableSpell)
        {
            observableSpell = null;

            if (WasCast) throw new Exception("Was already cast!");
            if ((this as TBindingScroll).IsCastable is false) throw new Exception("Not castable");

            library.AskForIngredient(out IObservable<T> ingredient);

            // TODO THIS IS A BIG CONSIDERATION TO MAKE
            // ARE SCROLLS CAST ONLY ONCE, ON LATER REQUESTS AN EXISTING 'CAST' IS PROVIDED
            // ALSO HANDLING SITUATION WHEN ONE OF IT'S INGREDIENTS IS KNOCKED OUT
            // ALSO HANDLING SITUATION WHEN THE CHANNEL IS NO LONGER CLOSED WITH A WEAVING
            // DOES THAT HAVE TO MATTER?
            // ARE SCROLLS CAST IF THE CHANNEL ISNT CLOSED?

            var noManaNotifier = Observable.Create<Q>(subscribe: observer =>
            {
                return HasMana
                    .Subscribe(onNext: hasMana =>
                    {
                        if (hasMana is false) observer.OnError(new NoManaException());
                    });
            });

            var erisTouchedIngredient = ingredient
                .Materialize()
                .Do(notification => { notification.Accept(eris.GetReceivalsObserver<T>(this)); })
                .Dematerialize();

            ObservableSpell = spell
                .Invoke(erisTouchedIngredient)
                .Merge(noManaNotifier)
                .Materialize()
                .Do(notification => { notification.Accept(eris.GetReleasesObserver<Q>(this)); })
                .Dematerialize();

             observableSpell = ObservableSpell;
        }

        public bool TryGetConjuring(out IObservable<Q> observableSpell)
        {
            observableSpell = null;

            if (WasCast)
            {
                observableSpell = ObservableSpell;
            }
            else
            {
                if (IsCastable)
                {
                    Cast(out observableSpell);
                }
            }

            return observableSpell != null;
        }

        public void Dispose()
        {
            _noManaObserverContract?.Dispose();
            eris.ScrollWillBeDisposed(this, isNew: false);
            library.ForgetLoomScroll<Q>(this);
        }
    }

    [Serializable]
    public class AlteringScroll<T> : TAlteringScroll where T : TMatter
    {
        readonly IObserver<T> spell;
        readonly TheLibrary library;
        readonly Eris eris;
        readonly object who;
        readonly Guid _guid = TScrollBase.CreateNewGuid();
        IDisposable _subscriptionDisposable;

        public AlteringScroll(object who, IObserver<T> spell, TheLibrary library, Eris eris)
        {
            this.who = who;
            this.spell = spell;
            this.library = library;
            this.eris = eris;
        }

        public Guid Guid => _guid;
        public string Title => $"Weaving of {(this as TBindingScroll).GetBindingScrollCode()}";

        public bool IsCastable
        {
            get
            {
                // TODO So the strong concept here was that Altering scrolls once cast are uncastable
                // TODO What happens if there appears a new or changed provider for the data they seek while they already opearate
                if (WasCast) return false;
                else return (this as TBindingScroll).AreAllIngredientsProvided;
            }
        }

        public Type[] Requirements { get; } = new[] { typeof(T) };
        public BehaviorSubject<bool> HasMana { get; } = new(false);

        public Dictionary<Type, bool> AvailableIngredientsDictionary { get; } = new(1)
        {
            { typeof(T), false }
        };

        public bool WasCast { get; private set; }
        public object Who => who;

        public void Dispose()
        {
            eris.ScrollWillBeDisposed(this, isNew: false);

            if (WasCast) _subscriptionDisposable.Dispose();
            else library.RemoveFromBlockedScrollsCollection(typeof(T), this);
        }

        public void Cast()
        {
            if ((this as TBindingScroll).IsCastable is false) throw new Exception("messed up");
            library.AskForIngredient<T>(out IObservable<T> ingredtient);

            IObservable<T> erisTouchedIngredient = ingredtient
                .Materialize()
                .Do(notification => { notification.Accept(eris.GetReceivalsObserver<T>(this)); })
                .Dematerialize();

            _subscriptionDisposable = erisTouchedIngredient
                .Subscribe(spell);

            WasCast = true;
        }
    }
}