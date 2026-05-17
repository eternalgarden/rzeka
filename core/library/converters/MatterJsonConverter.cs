using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rzeka.Serialization;

// Serializes IMatter using the concrete runtime type so user-defined properties
// (e.g. ScenePath, GameTitle) are included in the output. Without this,
// System.Text.Json only sees the two properties declared on IMatter itself.
public class MatterJsonConverter : JsonConverter<IMatter>
{
    public override void Write(Utf8JsonWriter writer, IMatter value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, value.GetType(), options);

    public override IMatter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException("IMatter deserialisation is not supported.");
}
