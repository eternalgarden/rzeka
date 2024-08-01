using System;
using Newtonsoft.Json;
using Rzeka.Serialization;

namespace Rzeka
{
    // TODO rework serializable spell occurence with this
    public interface ISerializableOccurence
    {
        OccurenceCategory occurenceCategory { get; } 
        Guid guid { get; }
        long timestamp  { get; } // In unix seconds
    }

    public interface ISerializableMatterOccurence : ISerializableOccurence
    {
        public MatterOccurenceCategory matterOccurenceCategory { get; }
        public Guid spellGuid { get; }
    }
    
    [Serializable]
    public abstract class SerializableMatterOccurence : ISerializableMatterOccurence
    {
        public OccurenceCategory occurenceCategory => OccurenceCategory.Matter;
        public abstract MatterOccurenceCategory matterOccurenceCategory { get; }
        public Guid guid { get; }
        public Guid spellGuid { get; }
        public long timestamp  { get; } // in unix seconds
        // public ISerializableSpell spell { get; set; } // TODO rework to just Guid
        // [JsonConverter(typeof(TypeJsonConverter))] public Type matterType { get; set; } // * we use a custom serializer for Type
        // public TMatter matter { get; set; } // TODO rework so only matter emission contains this

        public SerializableMatterOccurence(Guid guid, long timestamp, Guid spellGuid)
        {
            this.guid = guid;
            this.timestamp = timestamp;
            this.spellGuid = spellGuid;
        }

    }
    
    [Serializable]
    public class SerializableShapedMatter : SerializableMatterOccurence
    {
        public override MatterOccurenceCategory matterOccurenceCategory => MatterOccurenceCategory.Shaped;
        [JsonConverter(typeof(TypeJsonConverter))] public Type matterType { get; } // * we use a custom serializer for Type
        public TMatter matter { get; }

        public SerializableShapedMatter(
            Guid guid, 
            long timestamp, 
            Type matterType,
            Guid spellGuid,
            TMatter matter) 
            : base(guid, timestamp, spellGuid)
        {
            this.matter = matter;
            this.matterType = matterType;
        }
    }

    [Serializable]
    public class SerializableReceivedMatter : SerializableMatterOccurence
    {
        public override MatterOccurenceCategory matterOccurenceCategory => MatterOccurenceCategory.Received;
        public Guid receivedMatterGuid { get; set; }
        
        public SerializableReceivedMatter(
            Guid guid, 
            long timestamp,
            Guid spellGuid,
            Guid receivedMatterGuid) 
            : base(guid, timestamp, spellGuid)
        {
            this.receivedMatterGuid = receivedMatterGuid;
        }
    }
}