using System;
using System.Reactive.Linq;

namespace Rzeka;

[Serializable] // TODO remove serializable marks ... maybe?
public sealed class StrandingSpell<TOut> : IStrandingSpell<TOut> where TOut : IMatter
{
    public Guid Guid { get; }
    public object Who { get; }
    public SpellSchool SpellSchool => SpellSchool.Stranding;
    public string Title => $"Conjuring of {typeof(TOut).Name}";
    public ISpell ThisAsBase { get; }
    public IStrandingSpell<TOut> ThisAsStranding { get; }
    public IObservable<TOut> Conjuring { get; set; }
    public CollectibleDisposable Q { get; set; }
    public Type ConjuredType => typeof(TOut);
    public Library Library { get; }
    public Eris Eris { get; }
    
    
    IObservable<TOut> IStrandingSpell<TOut>.CreateConjuring()
    {
        return _spell
            .ObserveOn(Eris.MainThread)
            .Do(matter => ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped))
            .WhisperOnError(this);
    }

    IDisposable _libraryToken;

    bool _isChanneling;
    bool ISpell.HasMana
    {
        get => _isChanneling;
        set => _isChanneling = value;
    }
    
    readonly IObservable<TOut> _spell;

    public StrandingSpell(object who, IObservable<TOut> spell, Library library, Eris eris)
    {
        _spell = spell;
        
        Guid = Guid.NewGuid();
        Who = who;
        Eris = eris;
        Library = library;

        ThisAsBase = this;
        ThisAsStranding = this;

        ThisAsBase.InitializeSpellBase();
        ThisAsStranding.InitializeConjuringSpell();

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
        _libraryToken?.Dispose();
        Q?.Dispose();
        ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
    }
}
