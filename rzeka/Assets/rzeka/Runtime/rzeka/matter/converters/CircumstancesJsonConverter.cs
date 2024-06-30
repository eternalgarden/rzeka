using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rzeka.Serialization
{
    public class CircumstancesJsonConverter : JsonConverter<List<TMatter>>
    {
        
        public override void WriteJson(JsonWriter writer, List<TMatter> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Circumstances");
            writer.WriteStartArray();
            foreach (TMatter matter in value)
            {
                writer.WriteValue(matter.Guid);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public override List<TMatter> ReadJson(JsonReader reader, Type objectType, List<TMatter> existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException("This won't be implemented, can't be Read.");
        }
        
        public override bool CanRead => false;
    }
}