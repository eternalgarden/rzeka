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
        public IObservable<TOut> Conjuring { get; set; }

        public ISubject<SpellOccurence> SpellStream { get; }
        public ISubject<MatterOccurence> MatterStream { get; }        
        public CollectibleDisposable CollectionDisposable { get; set; }
        public Type ConjuredType => typeof(TOut);
        public Library Library { get; }
        
        
        IObservable<TOut> TConjuringSpell<TOut>.CreateConjuring()
        {
            return _spell
                .Do(matter => ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped));
        }

        IDisposable _libraryToken;

        bool _isChanneling;
        bool TSpell.IsChanneling
        {
            get => _isChanneling;
            set => _isChanneling = value;
        }
        
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

            // A Stranding spell is always channeling, it cant be blocked, its a pure giver
            _isChanneling = true;
            Cast();
        }

        public void Cast()
        {
            /* ⭐ ---- ---- */

            if (_libraryToken is not null) throw new Exception("Was already cast 🦇");

            try
            {
                _libraryToken = Library.RegisterConjurer(Conjuring);
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
            CollectionDisposable.Dispose();
            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
        }
    }
}