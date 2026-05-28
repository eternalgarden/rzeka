using System;

namespace Rzeka;
public interface ISerializableSpellOccurence : ISerializableOccurence
{
    public SpellOccurenceCategory spellOccurenceCategory { get; }
}

[Serializable]
public abstract class SerializableSpellOccurence : ISerializableSpellOccurence
{
    public OccurenceCategory occurenceCategory => OccurenceCategory.Spell;
    public abstract SpellOccurenceCategory spellOccurenceCategory { get; }
    public Guid guid { get; }
    public long timestamp  { get; } // in unix milliseconds

    public SerializableSpellOccurence(Guid guid, long timestamp)
    {
        this.guid = guid;
        this.timestamp = timestamp;
    }
}

public class SerializableCreatedSpellOccurence : SerializableSpellOccurence
{
    public override SpellOccurenceCategory spellOccurenceCategory => SpellOccurenceCategory.Created;
    
    public object spell { get; }

    public SerializableCreatedSpellOccurence(Guid guid, long timestamp, ISerializableSpell spell) : base(guid, timestamp)
    {
        this.spell = spell;
    }
}

public class SerializableOtherSpellOccurence : SerializableSpellOccurence
{
    public override SpellOccurenceCategory spellOccurenceCategory { get; }
    public Guid spellReference { get; }
    
    public SerializableOtherSpellOccurence(Guid guid, long timestamp, SpellOccurenceCategory category, Guid spellReference) : base(guid, timestamp)
    {
        this.spellReference = spellReference;
        this.spellOccurenceCategory = category;
    }
}
