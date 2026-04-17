using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rzeka.Serialization;
public class TypeJsonConverter : JsonConverter<Type>
{
    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("name", GetName(value));
        writer.WriteString("namespace", value.Namespace);
        writer.WriteEndObject();
    }

    string GetName(Type value)
    {
        if (value.IsGenericType)
        {
            StringBuilder builder = new();
            builder.Append("<");
            builder.AppendJoin(char.Parse(","), value.GenericTypeArguments.Select(x => x.Name));
            builder.Append(">");
            return builder.ToString();
        }
        else
            return value.Name;
    }

    public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("This won't be implemented, can't be Read.");
    }
}
