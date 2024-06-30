using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Rzeka
{
    [Serializable]
    public class LoomingSpell3<T1, T2, T3, TOut> : LoomingSpell<TOut>
        where TOut : TMatter
        where T1 : TMatter
        where T2 : TMatter
        where T3 : TMatter
    {
        readonly Func<IObservable<Glyph<T1, T2, T3>>, IObservable<TOut>> _spell;

        public LoomingSpell3(object who,
            Func<IObservable<Glyph<T1, T2, T3>>, IObservable<TOut>> spell,
            Library library,
            Eris eris) : base(who, library, eris)
        {
            this._spell = spell;

            InitializeLooming();
        }
        
        // TODO replace it with a string that will throw an error for end user that informs same type cant be used twice
        public override Dictionary<Type, bool> SatisfiedRequirements { get; } = new(3 )
        {
            { typeof(T1), false },
            { typeof(T2), false },
            { typeof(T3), false },
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
            
            var lastT3 = default(T3); // * attach last matter grabber
            IObservable<T3> ingredient3 = ThisAsBinding
                .GetObservableIngredient<T3>()
                .Do(nextT =>
                {
                    lastT3 = nextT;
                });

            IObservable<Glyph<T1, T2, T3>> observable = ingredient1
                .CombineLatest(ingredient2, ingredient3)
                .Select(anon => new Glyph<T1, T2, T3>(anon.First, anon.Second, anon.Third)); 

            var conjuring = _spell
                .Invoke(observable)
                .Select(matter =>
                {   
                    matter = matter.WithCircumstances<TOut>(lastT1, lastT2, lastT3);

                    ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped);

                    return matter;
                });
            
            return conjuring;
        }
    }
}