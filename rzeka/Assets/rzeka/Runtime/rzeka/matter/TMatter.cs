using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Rzeka.Serialization;

namespace Rzeka
{
    public interface TMatter : IEquatable<TMatter>
    {
        Guid Guid { get; } // TODO Do we need a public setter?
        List<TMatter> Circumstances { get; } // TODO This aswell does it need to be public?

        public bool HasCircumstances() => Circumstances.Count > 0;

        public T WithCircumstances<T>(params TMatter[] circumstances) where T : TMatter
        {
            T newMatter = (T)this; // TODO intentionally quick exception
            
            newMatter.Circumstances.Clear(); // trick for immutability
            newMatter.Circumstances.AddRange(circumstances);

            return newMatter;
        }

        bool IEquatable<TMatter>.Equals(TMatter other)
        {
            return other != null && this.Guid == other.Guid;
        }
    }

    public interface IRequest : TMatter { }
    
    public interface IResponse<out T> : TMatter where T : IRequest
    {
        T Request { get; }
        bool WasSuccessful { get; }
    }

    public abstract class Matter : TMatter
    {
        public Guid Guid { get; } = Guid.NewGuid();
        
        // This ignore is super important to prevent wild serialization blobbing +20MB text file wildness
        [JsonConverter(typeof(CircumstancesJsonConverter))] public List<TMatter> Circumstances { get; } = new();
    }
    
    [Serializable]
    public class SerializableMatter
    {
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
    }
    
    public abstract class Request : IRequest
    {
        public Guid Guid { get; } = Guid.NewGuid();
        public List<TMatter> Circumstances { get; } = new();
    }
    
    public abstract class Response<T> : IResponse<T> where T : IRequest
    {
        public Guid Guid { get; } = Guid.NewGuid();
        public List<TMatter> Circumstances { get; } = new();
        public T Request { get; }
        public bool WasSuccessful { get; }

        public Response(T request, bool wasSuccessful)
        {
            Request = request;
            WasSuccessful = wasSuccessful;
        }
    }
}
