using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using UnityEngine;

namespace Rzeka
{
    public class WeavingSpell3Glyph<T1,T2,T3> : WeavingBase
        where T1: TMatter
        where T2: TMatter
        where T3: TMatter
    {
        readonly Func<IObservable<Glyph<T1, T2, T3>>,IDisposable> _spell;

        public WeavingSpell3Glyph(object who,
            Func<IObservable<Glyph<T1, T2, T3>>,IDisposable> spell, Eris eris,
            Library library) : base(who, library, eris)
        {
            _spell = spell;
        }

        public override string Title => $"{Who.GetType().Name}'s Weaving of {typeof(T1).Name}, {typeof(T2).Name} & {typeof(T3).Name}";
        
        public override Dictionary<Type, bool> SatisfiedRequirements { get; } = new(3)
        {
            { typeof(T1), false },
            { typeof(T2), false },
            { typeof(T3), false },
        };
        
        public override void Cast()
        {
            /* ⭐ ---- ---- */
            
            var ingredientT1 = ThisAsBinding.GetObservableIngredient<T1>();
            var ingredientT2 = ThisAsBinding.GetObservableIngredient<T2>();
            var ingredientT3 = ThisAsBinding.GetObservableIngredient<T3>();

            if (ingredientT1 is null) throw new Exception($"Missing ingredient of type {typeof(T1)}");
            if (ingredientT2 is null) throw new Exception($"Missing ingredient of type {typeof(T2)}");
            if (ingredientT3 is null) throw new Exception($"Missing ingredient of type {typeof(T3)}");

            IObservable<Glyph<T1, T2, T3>> obs = ingredientT1
                .CombineLatest(ingredientT2, ingredientT3)
                .Select(comb => new Glyph<T1, T2, T3>
                {
                    One = comb.First,
                    Two = comb.Second,
                    Three = comb.Third,
                });
            
            CollectionDisposable += _spell.Invoke(obs);

            /* ---- ---- 🌠 */
        }
    }
}