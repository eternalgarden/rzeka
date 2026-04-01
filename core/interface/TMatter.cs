using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Rzeka.Serialization;

namespace Rzeka
{
    public interface TMatter : IEquatable<TMatter>
    {
        Guid Guid { get; }
        IReadOnlyList<TMatter> Circumstances { get; }

        TMatter Clone();
        TMatter Clone(params TMatter[] circumstances);

        public bool HasCircumstances() => Circumstances.Count > 0;

        public T WithCircumstances<T>(params TMatter[] circumstances)
            where T : TMatter
            => (T)Clone(circumstances);
    }

    public interface IRequest : TMatter { }

    public interface IResponse<out T> : TMatter
        where T : IRequest
    {
        T Request { get; }
        bool WasSuccessful { get; }
    }

    public class Matter : TMatter
    {
        public Guid Guid { get; } = Guid.NewGuid();

        private List<TMatter> _circumstances = new();

        /// <summary>
        /// This JsonConverter is super important to prevent wild serialization blobbing +20MB text file wildness
        /// </summary>
        [JsonConverter(typeof(CircumstancesJsonConverter))]
        public IReadOnlyList<TMatter> Circumstances => _circumstances;

        public virtual TMatter Clone()
        {
            var clone = (Matter)MemberwiseClone();
            clone._circumstances = new List<TMatter>(_circumstances);
            return clone;
        }

        public virtual TMatter Clone(params TMatter[] circumstances)
        {
            var clone = (Matter)MemberwiseClone();
            clone._circumstances = new List<TMatter>(circumstances);
            return clone;
        }

        #region IEquatable<TMatter>

        public bool Equals(TMatter other)
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

    public abstract class Request : Matter, IRequest { }

    public abstract class Response<T> : Matter, IResponse<T>
        where T : IRequest
    {
        public T Request { get; }
        public bool WasSuccessful { get; }

        protected Response(T request, bool wasSuccessful)
        {
            Request = request;
            WasSuccessful = wasSuccessful;
        }
    }
}
