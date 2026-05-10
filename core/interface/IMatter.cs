using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Rzeka.Serialization;

namespace Rzeka;
public interface IMatter : IEquatable<IMatter>
{
    Guid Guid { get; }
    IReadOnlyList<IMatter> Circumstances { get; }

    IMatter Clone();
    IMatter Clone(params IMatter[] circumstances);

    public bool HasCircumstances() => Circumstances.Count > 0;

    public T WithCircumstances<T>(params IMatter[] circumstances)
        where T : IMatter
        => (T)Clone(circumstances);
}

public abstract class Matter : IMatter
{
    public Guid Guid { get; } = Guid.NewGuid();

    private List<IMatter> _circumstances = new();

    /// <summary>
    /// This JsonConverter is super important to prevent wild serialization blobbing +20MB text file wildness
    /// </summary>
    [JsonConverter(typeof(CircumstancesJsonConverter))]
    public IReadOnlyList<IMatter> Circumstances => _circumstances;

    public virtual IMatter Clone()
    {
        var clone = (Matter)MemberwiseClone();
        clone._circumstances = new List<IMatter>(_circumstances);
        return clone;
    }

    public virtual IMatter Clone(params IMatter[] circumstances)
    {
        var clone = (Matter)MemberwiseClone();
        clone._circumstances = new List<IMatter>(circumstances);
        return clone;
    }

    #region IEquatable<IMatter>

    public bool Equals(IMatter other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Guid.Equals(other.Guid);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((Matter)obj);
    }

    public override int GetHashCode()
    {
        return Guid.GetHashCode();
    }

    #endregion
}
