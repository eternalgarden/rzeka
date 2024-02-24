using System;

namespace Rzeka
{
    public struct MatterOccurence : IOccurence
    {
        public Guid Guid { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TSpell Source { get; set; }
        public MatterOccurenceCategory MatterOccurenceCategory { get; set; }
        public TMatter Matter { get; set; }
    }
    
    // WARNING! VARIABLE NAMES CHANGE-SENSITIVE.
    // NAMING IS USED DIRECTLY IN UI-WEB
    public struct SerializableMatterOccurence
    {
        public OccurenceCategory occurenceCategory => OccurenceCategory.Matter;
        public Guid guid { get; set; }
        public long timestamp  { get; set; } // in unix seconds
        public ISerializableSpell spell { get; set; }
        public MatterOccurenceCategory matterOccurenceCategory { get; set; }
        public Type matterType { get; set; } // * we use a custom serializer for Type
        public TMatter matter { get; set; }
    }
}