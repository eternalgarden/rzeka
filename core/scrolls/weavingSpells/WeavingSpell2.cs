using System;
using System.Collections.Generic;

namespace Rzeka
{
    public class WeavingSpell2<T1, T2> : WeavingBase
        where T1 : TMatter
        where T2 : TMatter
    {
        readonly Func<IObservable<T1>, IObservable<T2>, IDisposable> _spell;

        public WeavingSpell2(
            object who,
            Func<IObservable<T1>, IObservable<T2>, IDisposable> spell,
            Eris eris,
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

            var ingredient1 = ThisAsBinding.GetObservableIngredient<T1>();
            var ingredient2 = ThisAsBinding.GetObservableIngredient<T2>();

            if (ingredient1 is null) throw new Exception($"Missing ingredient of type {typeof(T1)}");
            if (ingredient2 is null) throw new Exception($"Missing ingredient of type {typeof(T2)}");

            Q += _spell.Invoke(ingredient1, ingredient2);

            /* ---- ---- 🌠 */
        }
    }
}
