using System;
using System.Collections.Generic;

namespace Rzeka;
public abstract class WeavingBase : IWeavingSpell
{
    public Guid Guid { get; }
    public object Who { get; }
    public Library Library { get; }
    public Eris Eris { get; }
    public ISpell ThisAsBase  { get; }
    bool ISpell.HasMana { get; set; }
    public IBindingSpell ThisAsBinding { get; }
    public SpellSchool SpellSchool => SpellSchool.Weaving;
    public CollectibleDisposable Q { get; set; }
    
    public abstract string Title { get; }
    public abstract Dictionary<Type, bool> SatisfiedRequirements { get; }

    protected WeavingBase(
        object who, 
        Library library,
        Eris eris)
    {
        Guid = Guid.NewGuid();
        Library = library;
        Eris = eris;
        Who = who;
        ThisAsBase = this;
        ThisAsBinding = this;
        
        ThisAsBase.InitializeSpellBase();
        ThisAsBinding.InitializeBindingSpell();
    }

    public abstract void Cast();

    public virtual void Dispose()
    {
        Q.Dispose();
        ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
    }
}
