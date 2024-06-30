using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Rzeka.Serialization;

namespace Rzeka
{
    public interface TMatter : IEquatable<TMatter>
    {
        Guid Guid { get; } 
        List<TMatter> Circumstances { get; } 

        public bool HasCircumstances() => Circumstances.Count > 0;

        public T WithCircumstances<T>(params TMatter[] circumstances) where T : TMatter
        {
            var newMatter = (T)this; 
            
            newMatter.Circumstances.Clear(); // trick for immutability
            newMatter.Circumstances.AddRange(circumstances);

            return newMatter;
        }
    }

    public interface IRequest : TMatter { }
    
    public interface IResponse<out T> : TMatter where T : IRequest
    {
        T Request { get; }
        bool WasSuccessful { get; }
    }

    public class Matter : TMatter
    {
        public Guid Guid { get; } = Guid.NewGuid();
        
        /// <summary>
        /// This JsonConverter is super important to prevent wild serialization blobbing +20MB text file wildness
        /// </summary>
        [JsonConverter(typeof(CircumstancesJsonConverter))] public List<TMatter> Circumstances { get; } = new();

        #region IEquatable<TMatter>

        public bool Equals(TMatter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Guid.Equals(other.Guid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Matter)obj);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        #endregion
    }
    
    public abstract class Request : Matter, IRequest { }
    
    public abstract class Response<T> : Matter, IResponse<T> where T : IRequest
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
