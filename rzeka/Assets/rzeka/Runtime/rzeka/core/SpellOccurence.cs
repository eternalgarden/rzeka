using System;

namespace Rzeka
{
    public struct SpellOccurence : IOccurence
    {
        public Guid Guid { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TSpell Source { get; set; }

        public SpellOccurenceCategory SpellOccurenceCategory { get; set; }
    }
    
    public struct SerializableSpellOccurence
    {
        public OccurenceCategory occurenceCategory => OccurenceCategory.Spell;
        public SpellOccurenceCategory spellOccurenceCategory { get; set; }
        public Guid guid { get; set; }
        public long timestamp  { get; set; } // in unix milliseconds
        public ISerializableSpell spell { get; set; }
    }
}