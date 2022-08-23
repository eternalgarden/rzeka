using System;
using System.Collections.Generic;

namespace Rzeka
{

    public class ConjuringScroll<Q> : TScrollBase, TConjuringScroll<Q> where Q : TMatter
    {
        public IObservable<Q> spell;

        Guid _guid = TScrollBase.CreateNewGuid();
        public Guid Guid => _guid;

        public bool IsCastable => true;

        public Type ConjuredType => typeof(Q);

        public bool TryCast(out IObservable<Q> givingSpell, TheLibrary library)
        {
            givingSpell = spell;
            return true;
        }

        public bool TryCast(out object observableSpell, TheLibrary library)
        {
            observableSpell = spell;
            return true;
        }

        public void Dispose()
        {
            spell = null;
        }
    }

    public class LoomingScroll<T, Q> : TScrollBase, ILoomingScroll<Q> where Q : TMatter where T : TMatter
    {
        public Func<IObservable<T>, IObservable<Q>> spell;

        public Type[] Requirements { get; } = new[] { typeof(T) };

        public Dictionary<Type, bool> AvailableIngredientsDictionary { get; } = new(1)
            {
                { typeof(T), false }
            };

        public Type ConjuredType => typeof(Q);

        Guid _guid = TScrollBase.CreateNewGuid();
        public Guid Guid => _guid;

        public bool IsCastable => (this as TBindingScroll).AreAllIngredientsProvided;

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
        public IObserver<T> spell;
        Guid _guid = TScrollBase.CreateNewGuid();
        IDisposable _subscriptionDisposable;

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

        public void Dispose()
        {
            spell = null;

            if (WasCast) _subscriptionDisposable.Dispose();
        }

        public void Cast(TheLibrary library)
        {
            if ((this as TBindingScroll).IsCastable)
            {
                if (library.AskForIngredient<T>(out IObservable<T> ingredtient))
                {
                    _subscriptionDisposable = ingredtient.Subscribe(spell);
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
