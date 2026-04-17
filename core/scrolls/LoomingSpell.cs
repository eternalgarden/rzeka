using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace Rzeka;

/*

? Looming Scrolls Considerations

Looming Scroll will always have *at least* one dependency
                and always producing *one* type of matter.

* Hot vs Cold looms
* 
TODO basically all t
*/
public abstract class LoomingSpell<TOut> : TLoomingSpell<TOut>
    where TOut : TMatter
{
    public Guid Guid { get; }
    public object Who { get; }
    public Library Library { get; }
    public Eris Eris { get; }
    public virtual SpellSchool SpellSchool => SpellSchool.Looming;
    public abstract string Title { get; }
    public TSpell ThisAsBase  { get; }
    public TBindingSpell ThisAsBinding { get; }
    public TStrandingSpell<TOut> ThisAsStranding { get; }

    IObservable<TOut> TStrandingSpell<TOut>.Conjuring { get; set; }

    IObservable<TOut> TStrandingSpell<TOut>.CreateConjuring()
    {
        return CreateConjuring();
    }
    public CollectibleDisposable Q { get; set; }
    public Type ConjuredType => typeof(TOut);
    bool TSpell.HasMana { get; set; }

    // public bool WasCast => _conjurerLibraryToken is not null; // TODO rework maybe as 'IsActive' alon with the OnLostMana thing
    
    public abstract Dictionary<Type, bool> SatisfiedRequirements { get; }
    protected abstract IObservable<TOut> CreateConjuring();
    
    IDisposable _conjurerLibraryToken;

    protected LoomingSpell(
        object who,
        Library library,
        Eris eris)
    {
        Who = who;
        Library = library;
        Eris = eris;
        Guid = Guid.NewGuid();

        ThisAsBase = this;
        ThisAsBinding = this;
        ThisAsStranding = this;
    }

    protected void InitializeLooming()
    {
        ThisAsBase.InitializeSpellBase();
        ThisAsStranding.InitializeConjuringSpell();
        ThisAsBinding.InitializeBindingSpell();
        
        Cast(); // atm it is only used for looming scrolls ugh
    }


    public void Cast()
    {
        if (_conjurerLibraryToken is not null) throw new Exception("Was already cast 🦇");
        if (ThisAsStranding.Conjuring is null) throw new Exception("Conjuring is null");
        
        _conjurerLibraryToken = Library.RegisterConjurer<TOut>(ThisAsStranding.Conjuring);
    }
    
    // TODO VERY IMPORTANT, CHECK IF SPELLS ARE BEING DISPOSED CORRECTLY
    // * even though I warned myself 😭
    public virtual void Dispose()
    {
        UnregisterConjurerFromLibrary();
        Q.Dispose();
        ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
    }

    public ReplaySubject<bool> BindingHasMana { get; } = new();

    void UnregisterConjurerFromLibrary()
    {
        _conjurerLibraryToken?.Dispose();
        _conjurerLibraryToken = null;
    }
}
