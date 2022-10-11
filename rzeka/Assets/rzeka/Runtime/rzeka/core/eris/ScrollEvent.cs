using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rzeka.Internal;

namespace Rzeka
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ScrollEventType
    {
        New         = 1,
        Known         = 1 << 1,
        Cast        = 1 << 2,
        Blocked     = 1 << 3,
        Requested   = 1 << 4,
        Forgotten   = 1 << 5,
    }
    
    [Serializable]
    [JsonConverter(typeof(RealmEventJsonConverter))]
    public class ScrollEvent : RealmEvent
    {
        public readonly TScrollBase Scroll;
        public readonly ScrollEventType EventType;
        
        public ScrollEvent(TScrollBase scroll, ScrollEventType eventType) : base()
        {
            Scroll = scroll;
            EventType = eventType;
        }
    }
    
    // TODO NOTICE THIS ACTUALLY DOESNT MAKE SENSE, JUST MAKE ANONYMOUS OBJECTS "CONVERTER" METHODS
    // Maybe if I knew how to use newtonsoft better but their documentation is an outrageous garbage
    // How dare they considering how popular they scaled their project
    // <righteous anger>
    // * notice most interestingly that just defining such converter will make it to be the used one when
    // * calling plain JsonConvert.SerializeObject
    // public class ScrollEventConverter : JsonConverter<ScrollEvent>
    // {
    //     public override void WriteJson(JsonWriter writer, ScrollEvent value, JsonSerializer serializer)
    //     {
    //         var realmEvent = value as RealmEvent;
    //         
    //         writer.WriteStartObject();
    //         writer.WritePropertyName("Guid");
    //         writer.WriteValue(realmEvent.Guid);
    //         writer.WritePropertyName("Timestamp");
    //         writer.WriteValue(realmEvent.Timestamp);
    //         writer.WriteEndObject();
    //     }
    //
    //     public override ScrollEvent ReadJson(JsonReader reader, Type objectType, ScrollEvent existingValue, bool hasExistingValue,
    //         JsonSerializer serializer)
    //     {
    //         throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
    //     }
    //
    //     public override bool CanRead => false;
    // }
}