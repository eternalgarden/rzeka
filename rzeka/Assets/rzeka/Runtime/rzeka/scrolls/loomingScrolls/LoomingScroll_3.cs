using System;
using System.Collections.Generic;
using System.Reactive.Joins;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka
{
    [Serializable]
    public class LoomingScroll_3<T1, T2, T3, TOut> : LoomingScroll<TOut>
        where TOut : TMatter
        where T1 : TMatter
        where T2 : TMatter
        where T3 : TMatter
    {
        readonly Func<IObservable<Glyph<T1, T2, T3>>, IObservable<TOut>> _spell;

        public LoomingScroll_3(
            object who,
            Library library,
            Func<IObservable<Glyph<T1, T2, T3>>, IObservable<TOut>> spell,
            ISubject<SpellOccurence> spellStream, 
            ISubject<MatterOccurence> matterStream) : base(who, library, spellStream, matterStream)
        {
            this._spell = spell;

            ThisAsBinding.InitializeBindingSpell();
            ThisAsConjuring.InitializeConjuringSpell();
        }
        
        // TODO replace it with a string that will throw an error for end user that informs same type cant be used twice
        public override Dictionary<Type, bool> SatisfiedRequirements { get; } = new(1)
        {
            { typeof(T1), false },
            { typeof(T2), false },
            { typeof(T3), false },
        };

        protected override IDisposable CastSpell()
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
            
            IObservable<Glyph<T1,T2,T3>> observable = ingredient1
                .CombineLatest(ingredient2, ingredient3)
                .Select(anon => new Glyph<T1, T2, T3>() 
                    { One = anon.First, Two = anon.Second, Three = anon.Third});

            var conjuring = _spell
                .Invoke(observable)
                .Select(matter =>
                {   
                    matter = matter.WithCircumstances<TOut>(lastT1, lastT2, lastT3);

                    ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped);

                    return matter;
                });
            
            IDisposable token = Library.RegisterConjurer<TOut>(conjuring);

            return token;
        }
    }
}