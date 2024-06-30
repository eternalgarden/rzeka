using System;
using Newtonsoft.Json;
using Rzeka.Serialization;

namespace Rzeka
{
    public struct SerializableMatterOccurence
    {
        public OccurenceCategory occurenceCategory => OccurenceCategory.Matter;
        public Guid guid { get; set; }
        public long timestamp  { get; set; } // in unix seconds
        public ISerializableSpell spell { get; set; }
        public MatterOccurenceCategory matterOccurenceCategory { get; set; }
        [JsonConverter(typeof(TypeJsonConverter))] public Type matterType { get; set; } // * we use a custom serializer for Type
        public TMatter matter { get; set; }
    }
}