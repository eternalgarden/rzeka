using System;
using System.Linq;
using System.Reactive.Linq;

namespace Rzeka
{
    public interface TMatter
    {
        Guid Guid { get; }
        Guid[] Circumstances { get; set; }
        Type Type { get; } // * don't remove, it is used by Eris renderer
        string Description { get; }

        public bool HasCircumstances() => Circumstances.Length > 0;

        public void SetCircumstances(params TMatter[] circumstances)
        {
            Circumstances = circumstances.Select(x => x.Guid).ToArray();
        }

        public TMatter WithCircumstances<T,Y>(Glyph<T,Y> glyph)
            where T : TMatter
            where Y : TMatter
        {
            Circumstances = glyph.AsCircumstance();
            return this;
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
