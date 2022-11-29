using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{

    [Serializable] // TODO remove serializable marks
    public sealed class ConjuringScroll<Q> : TConjuringScroll<Q> where Q : TMatter
    {
        readonly IObservable<Q> spell;

        public Guid Guid { get; }
        public object Who { get; }
        public SpellType SpellType => SpellType.Stranding;
        public string Title => $"Conjuring of {typeof(Q).Name}";
        public TScrollBase ThisAsBase { get; }
        public TConjuringScroll<Q> ThisAsConjuring { get; }

        public ISubject<SpellOccurence> SpellStream { get; }
        public ISubject<MatterOccurence> MatterStream { get; }        
        public CollectibleDisposable CollectionDisposable { get; set; }
        public Type ConjuredType => typeof(Q);
        public IObservable<Q> ConjuredSpell { get; private set; }

        public ConjuringScroll(object who, IObservable<Q> spell, ISubject<SpellOccurence> spellStream, ISubject<MatterOccurence> matterStream)
        {
            this.spell = spell;
            
            Guid = Guid.NewGuid();
            Who = who;

            SpellStream = spellStream;
            MatterStream = matterStream;
            ThisAsBase = this;
            ThisAsConjuring = this;

            ThisAsConjuring.InitializeConjuringSpell();

            Cast();
        }

        public void Cast()
        {
            /* ⭐ ---- ---- */

            if (ThisAsBase.WasCast is true) throw new Exception("Was already cast 🦇");

            try
            {
                ConjuredSpell = spell;

                ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Cast);
            }
            catch (Exception ex)
            {
                // todo send luggage
                
                ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Wispd);
                // 
                // var wispd = new SpellOccurence
                // {
                //     SpellType = SpellType.Stranding,
                //     SpellOccurenceCategory = SpellOccurenceCategory.Wispd,
                //     Scroll = this,
                //     Luggage = new ExceptionalLuggage() { Exception = ex }
                // };

                // SpellStream.OnNext(wispd);
            }

            /* ---- ---- 🌠 */
        }

        public void Dispose()
        {
            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
            CollectionDisposable.Dispose();
        }
    }
}