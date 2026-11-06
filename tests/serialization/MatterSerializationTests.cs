using System;
using System.Text.Json;
using Rzeka;
using Rzeka.Serialization;

namespace Rzeka.Tests.Serialization;

public class MatterSerializationTests
{
    class MatterWithProperties : Matter
    {
        public string Label { get; }
        public int Count { get; }

        public MatterWithProperties(string label, int count)
        {
            Label = label;
            Count = count;
        }
    }

    class EmptyMatter : Matter { }

    class MatterWithCircumstances : Matter { }

    // Shaped like an engine vector: public fields, no properties.
    struct FieldVector
    {
        public float X;
        public float Y;
        public float Z;

        public FieldVector(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    class MatterWithFieldStruct : Matter
    {
        public FieldVector Point { get; }

        public MatterWithFieldStruct(FieldVector point) => Point = point;
    }

    // The dev server's own options, so these tests check what Eris actually receives.
    static JsonSerializerOptions SerializerOptions => Rzeka.Dev.RzekaDevExtensions.SerializerOptions;

    [Fact]
    public void ShapedMatter_StructWithPublicFields_IsSerializedWithItsFields()
    {
        var matter = new MatterWithFieldStruct(new FieldVector(1.5f, -2f, 3f));
        var occ = new SerializableShapedMatter(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            matter.GetType(),
            Guid.NewGuid(),
            matter
        );

        string json = JsonSerializer.Serialize(occ, occ.GetType(), SerializerOptions);
        using var doc = JsonDocument.Parse(json);
        var point = doc.RootElement.GetProperty("matter").GetProperty("Point");

        Assert.Equal(1.5f, point.GetProperty("X").GetSingle());
        Assert.Equal(-2f, point.GetProperty("Y").GetSingle());
        Assert.Equal(3f, point.GetProperty("Z").GetSingle());
    }

    [Fact]
    public void ShapedMatter_UserDefinedProperties_AreIncludedInJson()
    {
        var matter = new MatterWithProperties("hello", 42);
        var occ = new SerializableShapedMatter(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            matter.GetType(),
            Guid.NewGuid(),
            matter
        );

        string json = JsonSerializer.Serialize(occ, occ.GetType(), SerializerOptions);
        using var doc = JsonDocument.Parse(json);
        var matterNode = doc.RootElement.GetProperty("matter");

        Assert.Equal("hello", matterNode.GetProperty("Label").GetString());
        Assert.Equal(42, matterNode.GetProperty("Count").GetInt32());
    }

    [Fact]
    public void ShapedMatter_WithoutDescribeOwner_OnlyHasGuidAndCircumstances_WhenPropertiesAbsent()
    {
        var matter = new EmptyMatter();
        var occ = new SerializableShapedMatter(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            matter.GetType(),
            Guid.NewGuid(),
            matter
        );

        string json = JsonSerializer.Serialize(occ, occ.GetType(), SerializerOptions);
        using var doc = JsonDocument.Parse(json);
        var matterNode = doc.RootElement.GetProperty("matter");

        Assert.True(matterNode.TryGetProperty("Guid", out _));
        Assert.True(matterNode.TryGetProperty("Circumstances", out _));
    }

    [Fact]
    public void ShapedMatter_Circumstances_AreSerializedAsGuidStrings()
    {
        var circumstance = new MatterWithCircumstances();
        var matter = new MatterWithCircumstances().WithCircumstances<MatterWithCircumstances>(circumstance);
        var occ = new SerializableShapedMatter(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            matter.GetType(),
            Guid.NewGuid(),
            matter
        );

        string json = JsonSerializer.Serialize(occ, occ.GetType(), SerializerOptions);
        using var doc = JsonDocument.Parse(json);
        var circumstances = doc.RootElement.GetProperty("matter").GetProperty("Circumstances");

        Assert.Equal(JsonValueKind.Array, circumstances.ValueKind);
        Assert.Equal(1, circumstances.GetArrayLength());
        Assert.Equal(circumstance.Guid.ToString(), circumstances[0].GetString());
    }

    [Fact]
    public void ShapedMatter_Circumstances_AreNotSerializedAsNestedObjects()
    {
        var circumstance = new MatterWithCircumstances();
        var matter = new MatterWithCircumstances().WithCircumstances<MatterWithCircumstances>(circumstance);
        var occ = new SerializableShapedMatter(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            matter.GetType(),
            Guid.NewGuid(),
            matter
        );

        string json = JsonSerializer.Serialize(occ, occ.GetType(), SerializerOptions);
        using var doc = JsonDocument.Parse(json);
        var circumstances = doc.RootElement.GetProperty("matter").GetProperty("Circumstances");

        foreach (var element in circumstances.EnumerateArray())
            Assert.Equal(JsonValueKind.String, element.ValueKind);
    }
}
