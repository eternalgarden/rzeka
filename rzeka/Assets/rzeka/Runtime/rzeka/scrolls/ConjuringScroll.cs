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
        public Library Library { get; }
        public IObservable<TOut> ConjuredSpell { get; private set; } // ND

        IDisposable _libraryToken;
        public bool WasCast => _libraryToken is not null;
        
        readonly IObservable<TOut> _spell;

        public ConjuringScroll(object who, Library library, IObservable<TOut> spell, ISubject<SpellOccurence> spellStream, ISubject<MatterOccurence> matterStream)
        {
            _spell = spell;
            
            Guid = Guid.NewGuid();
            Who = who;
            Library = library;

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
                // ConjuredSpell = spell
                //     .Do(matter => ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped))
                //     .Multicast(new ReplaySubject<Q>(bufferSize: 1))
                //     .RefCount();

                var notifyingSpell = _spell
                    .Do(matter => ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped));

                // IDisposable token = Library.RegisterConjurer(
                //     notifyingSpell, 
                //     out var observable);
                //

                _libraryToken = Library.RegisterConjurer(notifyingSpell);
                
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
            _libraryToken.Dispose();
            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
            CollectionDisposable.Dispose();
        }
    }
}