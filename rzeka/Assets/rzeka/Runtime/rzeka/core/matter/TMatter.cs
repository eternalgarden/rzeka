using System;
using System.Linq;
using System.Reactive.Linq;

namespace Rzeka
{
    public interface TMatter
    {
        Guid Guid { get; set; }
        Guid[] Circumstances { get; set; }
        string Description { get; }

        public void SetCircumstances(params TMatter[] circumstances)
        {
            Circumstances = circumstances.Select(x => x.Guid).ToArray();
        }

        public void AssignNewGuid() => Guid = Guid.NewGuid();
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
