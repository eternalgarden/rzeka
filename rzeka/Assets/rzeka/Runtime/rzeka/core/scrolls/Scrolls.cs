using System;
using System.Collections.Generic;

namespace Rzeka
{

    public class Scroll<Q> : IScrollBase, TConjuringScroll<Q> where Q : TMatter
    {
        public IObservable<Q> spell;

        Guid _guid = new();
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

    public class Scroll<T, Q> : IScrollBase, TBindingScroll, TConjuringScroll<Q> where Q : TMatter where T : TMatter
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

    public class AlteringScroll<T> : IScrollBase, TAlteringScroll where T : TMatter
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

        public void Cast(TheLibrary library)
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
}
