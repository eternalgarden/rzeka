using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    [Serializable]
    public class LoomingScroll_1<T, Q> : LoomingScroll<Q>
        where Q : TMatter
        where T : TMatter
    {
        readonly Func<IObservable<T>, IObservable<Q>> spell;

        public LoomingScroll_1(
            object who,
            Func<IObservable<T>, IObservable<Q>> spell,
            ISubject<SpellOccurence> spellStream,
            ISubject<MatterOccurence> matterStream) : base(who, spellStream, matterStream)
        {
            this.spell = spell;

            ThisAsBinding.InitializeBindingSpell();
            ThisAsConjuring.InitializeConjuringSpell();
        }

        public override Dictionary<Type, List<IConjuringScroll>> Ingredients { get; } = new(1)
        {
            { typeof(T), new List<IConjuringScroll>() },
        };

        protected override IObservable<Q> GetConjuring()
        {
            /* ⭐ ---- ---- */

            T lastT = default(T);
            IObservable<T> ingredientT = ThisAsBinding
                .GetObservableIngredient<T>()
                .Do(nextT =>
                {
                    lastT = nextT;
                });

            IObservable<Q> conjuring = spell.Invoke(ingredientT)
                .Select(matter =>
                {

                    /* ⭐ ---- ---- */

                    matter = matter.WithCircumstances<Q>(lastT);

                    ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped);

                    return matter;

                    /* ---- ---- 🌠 */

                })
                .Multicast(new ReplaySubject<Q>(bufferSize: 1)) // ? provide alternatives
                .RefCount();

            return conjuring;

            /* ---- ---- 🌠 */
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}