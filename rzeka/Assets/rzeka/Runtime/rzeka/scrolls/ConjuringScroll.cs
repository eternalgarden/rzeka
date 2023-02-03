using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{

    [Serializable] // TODO remove serializable marks ... maybe?
    public sealed class ConjuringScroll<TOut> : TConjuringSpell<TOut> where TOut : TMatter
    {
        public Guid Guid { get; }
        public object Who { get; }
        public SpellSchool SpellSchool => SpellSchool.Stranding;
        public string Title => $"Conjuring of {typeof(TOut).Name}";
        public TSpell ThisAsBase { get; }
        public TConjuringSpell<TOut> ThisAsConjuring { get; }

        public ISubject<SpellOccurence> SpellStream { get; }
        public ISubject<MatterOccurence> MatterStream { get; }        
        public CollectibleDisposable CollectionDisposable { get; set; }
        public Type ConjuredType => typeof(TOut);
        public Library Library { get; set; }
        public IObservable<TOut> ConjuredSpell { get; private set; }
        
        readonly IObservable<TOut> spell;

        public ConjuringScroll(object who, IObservable<TOut> spell, ISubject<SpellOccurence> spellStream, ISubject<MatterOccurence> matterStream)
        {
            this.spell = spell;
            
            Guid = Guid.NewGuid();
            Who = who;

            SpellStream = spellStream;
            MatterStream = matterStream;
            ThisAsBase = this;
            ThisAsConjuring = this;

            ThisAsConjuring.InitializeConjuringSpell();
        }

        public void Cast()
        {
            /* ⭐ ---- ---- */

            if (ThisAsBase.WasCast is true) throw new Exception("Was already cast 🦇");

            try
            {
                // ConjuredSpell = spell
                //     .Do(matter => ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped))
                //     .Multicast(new ReplaySubject<Q>(bufferSize: 1))
                //     .RefCount();

                var notifyingSpell = spell
                    .Do(matter => ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped));

                IDisposable token = Library.RegisterConjurer(
                    notifyingSpell, 
                    out var observable);

                ConjuredSpell = observable;
                
                // A pure conjurer can only get inactive on its own disposal
                CollectionDisposable.Add(token);
                
                ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Cast);
            }
            catch (Exception ex)
            {
                // todo send luggage
                
                ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Wispd);
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