using System;
using System.Collections.Generic;
using System.Linq;

namespace Rzeka
{
    public interface IScrollBase : IDisposable
    {
        Guid Guid { get; }
        bool IsCastable { get; }
    }

    public interface TBindingScroll : IScrollBase
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

        bool IScrollBase.IsCastable => AvailableIngredientsDictionary.All(kvp => kvp.Value == true);
    }

    public interface TAlteringScroll : TBindingScroll
    {
        // todo remove try throw errr instgead
        void Cast(TheLibrary library);
    }

    public interface IConjuringScroll : IScrollBase
    {
        Type ConjuredType { get; }
        // todo remove try throw errr instgead
        // todo remove library reference this is dirty
        bool TryCast(out object observableSpell, TheLibrary library);
    }

    public interface TConjuringScroll<T> : IConjuringScroll where T : TMatter
    {
        bool TryCast(out IObservable<T> observableSpell, TheLibrary library);
    }
}