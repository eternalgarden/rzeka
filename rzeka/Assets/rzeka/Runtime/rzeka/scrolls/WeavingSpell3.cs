using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace Rzeka
{
    public class WeavingSpell3<T1,T2,T3> : WeavingBase
        where T1: TMatter
        where T2: TMatter
        where T3: TMatter
    {
        readonly Action<IObservable<T1>, IObservable<T2>, IObservable<T3>> _spell;

        public WeavingSpell3(object who,
            Action<IObservable<T1>, IObservable<T2>, IObservable<T3>> spell,
            Library library,
            Eris eris) : base(who, library, eris)
        {
            _spell = spell;
        }

        public override string Title => $"{Who.GetType().Name}'s Weaving of {typeof(T1).Name}";
        
        public override Dictionary<Type, bool> SatisfiedRequirements { get; } = new(1)
        {
            { typeof(T1), false },
        };
        
        public override void Cast()
        {
            /* ⭐ ---- ---- */
            
            var ingredientT1 = ThisAsBinding.GetObservableIngredient<T1>();
            if (ingredientT1 is null) throw new Exception($"Missing ingredient of type {typeof(T1)}");
            
            var ingredientT2 = ThisAsBinding.GetObservableIngredient<T2>();
            if (ingredientT2 is null) throw new Exception($"Missing ingredient of type {typeof(T2)}");
            
            var ingredientT3 = ThisAsBinding.GetObservableIngredient<T3>();
            if (ingredientT3 is null) throw new Exception($"Missing ingredient of type {typeof(T3)}");

            _spell.Invoke(ingredientT1, ingredientT2, ingredientT3);

            /* ---- ---- 🌠 */
        }
    }
}