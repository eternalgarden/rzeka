using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Rzeka
{
    [Serializable]
    public class InterlaceSpell3<T1, T2, T3, TOut> : LoomingSpell<TOut>
        where TOut : TMatter
        where T1 : TMatter
        where T2 : TMatter
        where T3 : TMatter
    {
        public override string Title =>
            $"Interlacing of {typeof(T1).Name}, {typeof(T2).Name} and {typeof(T3).Name}";

        readonly Func<IObservable<T1>, IObservable<T2>, IObservable<T3>, LoomContext, IObservable<TOut>> _spell;

        public InterlaceSpell3(
            object who,
            Func<IObservable<T1>, IObservable<T2>, IObservable<T3>, LoomContext, IObservable<TOut>> spell,
            Library library,
            Eris eris) : base(who, library, eris)
        {
            _spell = spell;
            InitializeLooming();
        }

        public override Dictionary<Type, bool> SatisfiedRequirements { get; } = new(3)
        {
            { typeof(T1), false },
            { typeof(T2), false },
            { typeof(T3), false },
        };

        protected override IObservable<TOut> CreateConjuring()
        {
            var lastT1 = default(T1);
            IObservable<T1> ingredient1 = ThisAsBinding
                .GetObservableIngredient<T1>()
                .Do(next => lastT1 = next);

            var lastT2 = default(T2);
            IObservable<T2> ingredient2 = ThisAsBinding
                .GetObservableIngredient<T2>()
                .Do(next => lastT2 = next);

            var lastT3 = default(T3);
            IObservable<T3> ingredient3 = ThisAsBinding
                .GetObservableIngredient<T3>()
                .Do(next => lastT3 = next);

            var ctx = new LoomContext(
                Eris,
                ThisAsBase,
                () => new TMatter[] { lastT1, lastT2, lastT3 }
            );

            return _spell
                .Invoke(ingredient1, ingredient2, ingredient3, ctx)
                .Select(matter =>
                {
                    matter = matter.WithCircumstances<TOut>(lastT1, lastT2, lastT3);
                    ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped);
                    return matter;
                });
        }
    }
}
