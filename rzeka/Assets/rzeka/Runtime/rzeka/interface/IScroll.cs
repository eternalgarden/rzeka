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
        object Who { get; }
        bool IsCastable { get; }
        bool WasCast { get; }
        
        void Cast();
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
                if (AvailableIngredientsDictionary.ContainsKey(type) is false) throw new Exception("process error");


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

    public interface IAlteringScroll : TBindingScroll
    {
    }

    public interface ILoomingScroll : TBindingScroll, IConjuringScroll
    {
        
    }

    public interface ILoomingScroll<Q> : ILoomingScroll, TConjuringScroll<Q>
        where Q : TMatter
    {
    }

    public interface TConjuringScroll<Q> : IConjuringScroll where Q : TMatter
    {
        /// <summary>
        /// This hides an important architecture decision that the conjuring spells may only be cast once.
        /// To re-cast one it's scroll would have to be disposed and summoned again, unless it has gotten out of mana and then it is be provided with it again.
        /// TODO CONSIDER ADDING ReCast() INTERFACE METHOD
        /// </summary>
        IObservable<Q> ConjuredSpell { get; }
    }

    public interface IConjuringScroll : TScrollBase
    {
        Type ConjuredType { get; }
    }
}