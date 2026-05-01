using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rzeka.Serialization;
public class CircumstancesJsonConverter : JsonConverter<IReadOnlyList<IMatter>>
{
    public override void Write(Utf8JsonWriter writer, IReadOnlyList<IMatter> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (IMatter matter in value)
        {
            writer.WriteStringValue(matter.Guid);
        }
        writer.WriteEndArray();
    }

    public override IReadOnlyList<IMatter> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("This won't be implemented, can't be Read.");
    }
}
