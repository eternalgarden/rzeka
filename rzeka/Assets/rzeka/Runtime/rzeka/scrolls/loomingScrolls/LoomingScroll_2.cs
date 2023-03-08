using System;
using System.Collections.Generic;
using System.Reactive.Joins;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka
{
    [Serializable]
    public class LoomingScroll_2<T1, T2, TOut> : LoomingScroll<TOut>
        where TOut : TMatter
        where T1 : TMatter
        where T2 : TMatter
    {
        readonly Func<IObservable<Glyph<T1, T2>>, IObservable<TOut>> _spell;

        public LoomingScroll_2(object who,
            Func<IObservable<Glyph<T1, T2>>, IObservable<TOut>> spell,
            Library library,
            Eris eris) : base(who, library, eris)
        {
            this._spell = spell;

            InitializeLooming();
        }

        public override Dictionary<Type, bool> SatisfiedRequirements { get; } = new(2)
        {
            { typeof(T1), false },
            { typeof(T2), false },
        };

        protected override IObservable<TOut> CreateConjuring()
        {
            var lastT1 = default(T1); // * attach last matter grabber
            IObservable<T1> ingredient1 = ThisAsBinding
                .GetObservableIngredient<T1>()
                .Do(nextT =>
                {
                    lastT1 = nextT;
                });
            
            var lastT2 = default(T2); // * attach last matter grabber
            IObservable<T2> ingredient2 = ThisAsBinding
                .GetObservableIngredient<T2>()
                .Do(nextT =>
                {
                    lastT2 = nextT;
                });
            
            IObservable<Glyph<T1,T2>> observable = ingredient1
                .CombineLatest(ingredient2)
                .Select(anon => new Glyph<T1, T2>() { One = anon.First, Two = anon.Second });

            var conjuring = _spell
                .Invoke(observable)
                .Select(matter =>
                {   
                    matter = matter.WithCircumstances<TOut>(lastT1, lastT2);

                    ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped);

                    return matter;
                });
            
            return conjuring;
        }
    }
}