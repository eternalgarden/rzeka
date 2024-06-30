using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Rzeka.Serialization
{
    public class TypeJsonConverter : JsonConverter<Type>
    {
        bool IsBaseTypeSystemType(Type type)
        {
            Debug.Assert(type.FullName != null, "type.FullName != null");
            return type.FullName.StartsWith("System.");
        }
        
        public override void WriteJson(JsonWriter writer, Type value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(GetName(value));
            writer.WritePropertyName("namespace");
            writer.WriteValue(value.Namespace);
            writer.WritePropertyName("baseTypeName");
            // writer.WriteValue(IsBaseTypeSystemType(value) ? "None" : value.BaseType);
            writer.WriteValue(value.BaseType is not null ? value.BaseType.FullName : "null");
            writer.WriteEndObject();
        }

        string GetName(Type value)
        {
            if (value.IsGenericType)
            {
                UnityEngine.Debug.Log($"<color=orange>xxx</color>");
                StringBuilder builder = new();
                builder.Append("<");
                builder.AppendJoin(char.Parse(","), value.GenericTypeArguments.Select(x => x.Name));
                builder.Append(">");
                
                UnityEngine.Debug.Log($"<color=yellow>{builder.ToString()}</color>");
                return builder.ToString();
            }
            else return value.Name;
        }

        public override Type ReadJson(JsonReader reader, Type objectType, Type existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException("This won't be implemented, can't be Read.");
        }
        
        public override bool CanRead => false;
    }
}