using System;
using System.Collections.Generic;

namespace Rzeka
{
    public class WeavingSpell1<T1> : WeavingBase
        where T1: TMatter
    {
        readonly Func<IObservable<T1>,IDisposable> _spell;

        public WeavingSpell1(object who,
            Func<IObservable<T1>,IDisposable> spell, Eris eris,
            Library library) : base(who, library, eris)
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

            Q += _spell.Invoke(ingredientT1);

            /* ---- ---- 🌠 */
        }
    }
}