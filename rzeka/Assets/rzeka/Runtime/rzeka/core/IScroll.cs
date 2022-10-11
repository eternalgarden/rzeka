using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

namespace Rzeka
{
    public interface TScrollBase : IDisposable
    {
        Guid Guid { get; }
        string Title { get; }
        bool IsCastable { get; }
        object Who { get; }

        protected static Guid CreateNewGuid() => Guid.NewGuid();
    }

    public interface TBindingScroll : TScrollBase
    {
        Dictionary<Type, bool> AvailableIngredientsDictionary { get; }
        Type[] Requirements { get; }
        BehaviorSubject<bool> HasMana { get; }

        public bool this[Type type]
        {
            get
            {
                if (AvailableIngredientsDictionary.ContainsKey(type) is false)
                    throw new Exception("Unexpected requested type. Did you check requirements first.");

                return AvailableIngredientsDictionary[type];
            }
            set
            {
                if (AvailableIngredientsDictionary.ContainsKey(type) is false) return;

                AvailableIngredientsDictionary[type] = value;
                
                bool hasMana = AreAllIngredientsProvided;
                if (HasMana.Value != hasMana) HasMana.OnNext(hasMana);
            }
        }

        bool AreAllIngredientsProvided => AvailableIngredientsDictionary
            .All(kvp => kvp.Value == true);

        string GetBindingScrollCode()
        {
            string code = "";

            for (int i = 0; i < Requirements.Length; i++)
            {
                code += Requirements[i].Name;
                if (i < Requirements.Length - 1) code += ", ";
            }

            return code;
        }
    }

    public interface TAlteringScroll : TBindingScroll
    {
        bool WasCast { get; }
        void Cast();
    }

    public interface ILoomingScroll<Q> : TBindingScroll, TConjuringScroll<Q>
        where Q : TMatter
    {
    }

    public interface TConjuringScroll<T> : IConjuringScroll where T : TMatter
    {
        bool TryGetConjuring(out IObservable<T> observableSpell);
    }

    public interface IConjuringScroll : TScrollBase
    {
        Type ConjuredType { get; }
    }
}