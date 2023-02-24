using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    [Serializable]
    public class LoomingScroll_1<T1, TOut> : LoomingScroll<TOut>
        where TOut : TMatter
        where T1 : TMatter
    {
        readonly Func<IObservable<T1>, IObservable<TOut>> _spell;

        public LoomingScroll_1(object who,
            Func<IObservable<T1>, IObservable<TOut>> spell,
            Library library,
            Eris eris) : base(who, library, eris)
        {
            _spell = spell;

            InitializeLooming();
        }
        
        public override Dictionary<Type, bool> SatisfiedRequirements { get; } = new(1)
        {
            { typeof(T1), false },
        };

        protected override IObservable<TOut> CreateConjuring()
        {
            /* ⭐ ---- ---- */
            
            var lastT = default(T1); // * attach last matter grabber
            IObservable<T1> ingredientT = ThisAsBinding
                .GetObservableIngredient<T1>()
                .Do(nextT =>
                {
                    lastT = nextT;
                });

            var conjuring = _spell
                .Invoke(ingredientT)
                .Select(matter =>
                {

                    /* ⭐ ---- ---- */
                    
                    // * Set circumstances to the last grabbed 
                    // ! This means multithreading or async operators can't be used in rzeka communication
                    matter = matter.WithCircumstances<TOut>(lastT);

                    ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped);

                    return matter;

                    /* ---- ---- 🌠 */

                });
                // nd
                // TODO this will be deleted since Library will handle that
                // .Multicast(new ReplaySubject<TOut>(bufferSize: 1)) // ? provide alternatives
                // .RefCount();

            return conjuring;

            /* ---- ---- 🌠 */
        }
    }
}