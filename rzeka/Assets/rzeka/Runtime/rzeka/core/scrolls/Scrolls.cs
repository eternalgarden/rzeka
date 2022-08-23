using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Rzeka
{

    public class ConjuringScroll<Q> : TScrollBase, TConjuringScroll<Q> where Q : TMatter
    {
        private readonly IObservable<Q> spell;
        private readonly TheLibrary library;
        private readonly Eris eris;
        private readonly object who;
        private readonly object wisp = new();

        public ConjuringScroll(object who, IObservable<Q> spell, TheLibrary library, Eris debugger)
        {
            this.who = who;
            this.spell = spell;
            this.library = library;
            this.eris = debugger;
        }

        Guid _guid = TScrollBase.CreateNewGuid();
        public Guid Guid => _guid;

        public bool IsCastable => true;

        public Type ConjuredType => typeof(Q);

        public object Who => who;

        public bool WasDisposed { get; private set; }

        public bool TryCast(out IObservable<Q> givingSpell, TheLibrary library)
        {
            givingSpell = spell
                        .Materialize()
                        .Do(notification =>
                        {
                            notification.Accept(eris.GetReleasesObserver<Q>(this));
                        })
                        .Dematerialize();
            return true;
        }

        public bool TryCast(out object observableSpell, TheLibrary library)
        {
            observableSpell = spell
                        .Materialize()
                        .Do(notification =>
                        {
                            notification.Accept(eris.GetReleasesObserver<Q>(this));
                        })
                        .Dematerialize();

            return true;
        }

        public void Dispose()
        {
            lock (wisp)
            {
                if (WasDisposed is false)
                {
                    eris.ScrollWillBeDisposed(this);
                    library.RemoveFromConjuringScrolls(this);

                    WasDisposed = true;
                }
            }
        }
    }

    public class LoomingScroll<T, Q> : TScrollBase, ILoomingScroll<Q> where Q : TMatter where T : TMatter
    {
        private readonly Func<IObservable<T>, IObservable<Q>> spell;
        private readonly TheLibrary library;
        private readonly Eris eris;
        private readonly object who;
        private readonly object wisp = new();

        public LoomingScroll(object who, Func<IObservable<T>, IObservable<Q>> spell, TheLibrary library, Eris eris)
        {
            this.who = who;
            this.spell = spell;
            this.library = library;
            this.eris = eris;
        }

        public Type[] Requirements { get; } = new[] { typeof(T) };

        public Dictionary<Type, bool> AvailableIngredientsDictionary { get; } = new(1)
            {
                { typeof(T), false }
            };

        public Type ConjuredType => typeof(Q);

        Guid _guid = TScrollBase.CreateNewGuid();
        public Guid Guid => _guid;

        public bool IsCastable => (this as TBindingScroll).AreAllIngredientsProvided;

        public object Who => who;
        public bool WasDisposed { get; private set; }

        public bool TryCast(out object observableSpell, TheLibrary library)
        {
            bool success = TryCast(out IObservable<Q> spell, library);
            observableSpell = spell;
            return success;
        }

        public bool TryCast(out IObservable<Q> observableSpell, TheLibrary library)
        {
            observableSpell = null;

            if ((this as TBindingScroll).IsCastable)
            {
                if (library.AskForIngredient<T>(out IObservable<T> ingredtient))
                {
                    IObservable<T> erisTouchedIngredient = ingredtient
                        .Materialize()
                        .Do(notification =>
                        {
                            notification.Accept(eris.GetReceivalsObserver<T>(this));
                        })
                        .Dematerialize();

                    observableSpell = spell
                        .Invoke(erisTouchedIngredient)
                        .Materialize()
                        .Do(notification =>
                        {
                            notification.Accept(eris.GetReleasesObserver<Q>(this));
                        })
                        .Dematerialize();
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
            lock (wisp)
            {
                if (WasDisposed is false)
                {
                    eris.ScrollWillBeDisposed(this);
                    library.ForgetLoomScroll<Q>(this);

                    WasDisposed = true;
                }
            }
        }
    }

    public class AlteringScroll<T> : TScrollBase, TAlteringScroll where T : TMatter
    {
        private readonly IObserver<T> spell;
        private readonly TheLibrary library;
        private readonly Eris eris;
        private readonly object who;
        private readonly object wisp = new();

        Guid _guid = TScrollBase.CreateNewGuid();
        IDisposable _subscriptionDisposable;

        public AlteringScroll(object who, IObserver<T> spell, TheLibrary library, Eris eris)
        {
            this.who = who;
            this.spell = spell;
            this.library = library;
            this.eris = eris;
        }

        public Guid Guid => _guid;

        public Type[] Requirements { get; } = new[] { typeof(T) };

        public Dictionary<Type, bool> AvailableIngredientsDictionary { get; } = new(1)
            {
                { typeof(T), false }
            };

        public bool WasCast { get; private set; }

        public bool IsCastable
        {
            get
            {
                if (WasCast) return false;
                else return (this as TBindingScroll).AreAllIngredientsProvided;
            }
        }

        public object Who => who;
        public bool WasDisposed { get; private set; }

        public void Dispose()
        {
            lock (wisp)
            {
                if (WasDisposed is false)
                {
                    eris.ScrollWillBeDisposed(this);

                    if (WasCast) _subscriptionDisposable.Dispose();
                    else library.RemoveFromBlockedScrollsCollection(typeof(T), this);

                    WasDisposed = true;
                }
            }
        }

        public void Cast(TheLibrary library)
        {
            if ((this as TBindingScroll).IsCastable)
            {
                if (library.AskForIngredient<T>(out IObservable<T> ingredtient))
                {
                    IObservable<T> erisTouchedIngredient = ingredtient
                        .Materialize()
                        .Do(notification =>
                        {
                            notification.Accept(eris.GetReceivalsObserver<T>(this));
                        })
                        .Dematerialize();

                    _subscriptionDisposable = erisTouchedIngredient
                        .Subscribe(spell);

                    WasCast = true;
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
}
