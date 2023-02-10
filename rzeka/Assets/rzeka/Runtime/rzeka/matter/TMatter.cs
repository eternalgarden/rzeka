using System;
using System.Linq;
using System.Reactive.Linq;
using UnityEngine;

namespace Rzeka
{
    public interface TMatter : IEquatable<TMatter>
    {
        Guid Guid { get; set; }
        Guid[] Circumstances { get; set; }
        [Obsolete] Type Type { get; } // todo REMOVE NO LONGER USED, THIS WAS A BAD IDEA
        string Description { get; }

        public bool HasCircumstances() => Circumstances.Length > 0;

        public T WithCircumstances<T>(params TMatter[] circumstances) where T : TMatter
        {
            T newMatter = (T)this; // intentionally quick exception

            Guid[] newCircumstances = circumstances.Select(x => x.Guid).ToArray();
            newMatter.Circumstances = newCircumstances;

            return newMatter;
        }

        bool IEquatable<TMatter>.Equals(TMatter other)
        {
            return this.Guid == other.Guid;
        }
    }

    public interface IRequest : TMatter { }
    public interface IResponse<T> : TMatter where T : IRequest
    {
        T Request { get; set; }
    }

    public interface TAllowingMultipleSources<T> where T : TMatter
    {
        public IObservable<T> CombineSourcesPattern(params IObservable<T>[] sources)
            => Observable.Merge(sources);
    }

    public interface ICacheLast : IHasDefaultValue
    {
        bool TreasureData { get; } // TODO Should keep that data if noone listens to it anymore
        //WindowSize 
    }

    public interface ICacheBuffer
    {
        int BufferSize { get; }

    }

    public interface IHasDefaultValue
    {
        void SetToDefaultValues();
    }
}
