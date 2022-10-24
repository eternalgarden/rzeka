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

    public class SpellInterruptedException : Exception
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
        IObservable<Q> _observableSpell;

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
        public bool IsConjured => ObservableSpell is not null;

        IObservable<Q> ObservableSpell
        {
            get => _observableSpell;
            set
            {
                if (_observableSpell is not null) throw new Exception("Was already cast");

                _observableSpell = value;
            }
        }

        public void Cast() 
        {
            if (IsConjured) return;

            ObservableSpell = spell
                .Materialize()
                .Do(notification => { notification.Accept(eris.GetReleasesObserver<Q>(this)); })
                .Dematerialize();
        }

        public IObservable<Q> GetConjuring()
        {
            if (IsConjured is false)
            {
                Cast();
            }

            return ObservableSpell;
        }

        public void Dispose()
        {
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
        public bool IsConjured => ObservableSpell is not null;
        public Type[] Requirements { get; } = { typeof(T) };

        // TODO hide those shouldnt be public
        public BehaviorSubject<bool> HasMana { get; } = new(false);

        public Dictionary<Type, bool> AvailableIngredientsDictionary { get; } = new(1)
        {
            { typeof(T), false }
        };

        public object Who => who;

        IDisposable _noManaObserverContract;
        IObservable<Q> _observableSpell;


        // TODO WRITE TESTS FOR THIS
        // THINKING HOW NOMANA WORKS IS ALSO NECESSARY
        // POSSIBLY ADDITIONAL CALLS ON DISPOSE

        // TODO A NECESSARY RE-CAST ONCE MANA IS PROVIDED
        // COULD THIS BE AUTOMATIC WITHOUT INPUT FROM THE LIBRARY THAT SPELLS CAST THEMSELVES ONCE PROVIDED WITH MANA
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
                                Debug.Log("caught no mana in conjuring");

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

        public IObservable<Q> GetConjuring()
        {
            if (!IsConjured && !IsCastable)
            {
                throw new Exception("Cant be conjured");
            }

            if (!IsConjured)
            {
                Cast();
            }

            return ObservableSpell;
        }

        // LOOMING SCROLL IS BASICALLY ONLY CAST FROM THE LIBRARY AS AskForIngredient 
        // WHEN IT WAS BLOCKED
        // IF IT WASNT BLOCKED IT WILL BE CAST IMMEDIATELY
        // TODO WHAT IF IT ISNT USED BY ANY WEAVING, WHY WOULD IT BE CAST THEN
        // TODO IT NEEDS CLEANING 
        public void Cast()
        {
            if (IsConjured) throw new Exception("Was already cast!");
            if ((this as TBindingScroll).IsCastable is false) throw new Exception("Not castable");

            library.AskForIngredient(out IObservable<T> ingredient);

            // TODO THIS IS A BIG CONSIDERATION TO MAKE
            // ARE SCROLLS CAST ONLY ONCE, ON LATER REQUESTS AN EXISTING 'CAST' IS PROVIDED
            // ALSO HANDLING SITUATION WHEN ONE OF IT'S INGREDIENTS IS KNOCKED OUT
            // ALSO HANDLING SITUATION WHEN THE CHANNEL IS NO LONGER CLOSED WITH A WEAVING
            // DOES THAT HAVE TO MATTER?
            // ARE SCROLLS CAST IF THE CHANNEL ISNT CLOSED?

            // TODO OKI, SO FAR A CHANGE IN INGREDIENTS WILL BE PROPAGATED FURTHER
            // BUT WHAT IF THIS SCROLL ITSELF IS BECOMING AN INACTIVE/DISPOSED COMPONENT
            // WHERE DOES THE INGREDIENT WATERFALL BEGIN, IT'S NOT CLEAR
            var noManaNotifier = Observable.Create<Q>(subscribe: observer =>
            {
                return HasMana
                    .Where(hasMana => hasMana is false)
                    .Subscribe(onNext: _ =>
                    {
                        Debug.Log("throwing no mana in loom");
                        observer.OnError(new NoManaException());
                    });
            });
            
            // TODO an attempt at circumstances
            T lastCircumstance = default(T);

            var erisTouchedIngredient = ingredient
                // TODO couldn't circumstances be intercepted here?
                .Do(onNext: next => lastCircumstance = next)
                .Materialize()
                .Do(notification => { notification.Accept(eris.GetReceivalsObserver<T>(this)); })
                .Dematerialize();

            ObservableSpell = spell
                .Invoke(erisTouchedIngredient)
                .Merge(noManaNotifier)
                // TODO So before I thought I had this perfect solution to handle circumstances but where is it
                // .Do(onNext: matter => matter.SetCircumstances())
                .Do(onNext: next => next.SetCircumstances(lastCircumstance))
                .Materialize()
                .Do(notification => { notification.Accept(eris.GetReleasesObserver<Q>(this)); })
                .Dematerialize();
        }

        public void Dispose()
        {
            _noManaObserverContract?.Dispose();
        }
    }

    [Serializable]
    public class AlteringScroll<T> : IAlteringScroll where T : TMatter
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

                return (this as TBindingScroll).AreAllIngredientsProvided;
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
            if (WasCast) _subscriptionDisposable.Dispose(); // TODO check this
        }

        public void Cast()
        {
            if ((this as TBindingScroll).IsCastable is false) throw new Exception("messed up");

            var noManaNotifier = Observable.Create<T>(subscribe: observer =>
            {
                return HasMana
                    .Where(hasMana => hasMana is false)
                    .Subscribe(onNext: _ =>
                    {
                        Debug.Log("throwing no mana in weave");
                        observer.OnError(new NoManaException());
                    });
            });

            library.AskForIngredient<T>(out IObservable<T> ingredient);
            var erisTouchedIngredient = ingredient
                .Materialize()
                .Do(notification => { notification.Accept(eris.GetReceivalsObserver<T>(this)); })
                .Dematerialize();

            _subscriptionDisposable = erisTouchedIngredient
                .Merge(noManaNotifier)
                .Subscribe(spell.OnNext, err =>
                {
                    if (err is NoManaException)
                    {
                        // THE SPELL BECOMES INACTIVE/BLOCKED
                        _subscriptionDisposable.Dispose();
                    }
                    else
                    {
                        spell.OnError(err);
                    }
                }, spell.OnCompleted);

            WasCast = true;
        }
    }
}