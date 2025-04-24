using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Rzeka
{
    public class WeavingSpell2Glyph<T1,T2> : WeavingBase
        where T1: TMatter
        where T2: TMatter
    {
        readonly Func<IObservable<Glyph<T1, T2>>,IDisposable> _spell;

        public WeavingSpell2Glyph(object who,
            Func<IObservable<Glyph<T1, T2>>,IDisposable> spell, Eris eris,
            Library library) : base(who, library, eris)
        {
            _spell = spell;
        }

        public override string Title => $"{Who.GetType().Name}'s Weaving of {typeof(T1).Name} & {typeof(T2).Name}";
        
        public override Dictionary<Type, bool> SatisfiedRequirements { get; } = new(2)
        {
            { typeof(T1), false },
            { typeof(T2), false },
        };
        
        public override void Cast()
        {
            /* ⭐ ---- ---- */
            
            var ingredientT1 = ThisAsBinding.GetObservableIngredient<T1>();
            var ingredientT2 = ThisAsBinding.GetObservableIngredient<T2>();

            if (ingredientT1 is null) throw new Exception($"Missing ingredient of type {typeof(T1)}");
            if (ingredientT2 is null) throw new Exception($"Missing ingredient of type {typeof(T2)}");

            IObservable<Glyph<T1, T2>> obs = ingredientT1
                .CombineLatest(ingredientT2)
                .Select(comb => new Glyph<T1, T2>(comb.First, comb.Second));
            
            Q += _spell.Invoke(obs);

            /* ---- ---- 🌠 */
        }
    }
}