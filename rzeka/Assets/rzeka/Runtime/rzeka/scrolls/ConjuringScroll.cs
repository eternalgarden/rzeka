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
        public CollectibleDisposable CollectionDisposable { get; set; }
        public Type ConjuredType => typeof(TOut);
        public Library Library { get; }
        public Eris Eris { get; }
        
        
        IObservable<TOut> TConjuringSpell<TOut>.CreateConjuring()
        {
            return _spell
                .Do(matter => ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped));
        }

        IDisposable _libraryToken;

        bool _isChanneling;
        bool TSpell.HasMana
        {
            get => _isChanneling;
            set => _isChanneling = value;
        }
        
        readonly IObservable<TOut> _spell;

        public ConjuringScroll(object who, IObservable<TOut> spell, Library library, Eris eris)
        {
            _spell = spell;
            
            Guid = Guid.NewGuid();
            Who = who;
            Eris = eris;
            Library = library;

            ThisAsBase = this;
            ThisAsConjuring = this;

            ThisAsBase.InitializeSpellBase();
            ThisAsConjuring.InitializeConjuringSpell();

            // A Stranding spell is always channeling, it cant be blocked, its a pure giver
            _isChanneling = true;
            Cast();
        }

        public void Cast()
        {
            /* ⭐ ---- ---- */

            if (_libraryToken is not null) throw new Exception("Was already cast 🦇");

            // try
            // {
                _libraryToken = Library.RegisterConjurer(Conjuring);
                ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.HasMana);
            // }
            // catch (Exception ex)
            // {
            //     // todo send luggage
            //     throw ex;
            //     
            //     ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Wispd);
            // }

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