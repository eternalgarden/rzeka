using System;
using System.Reactive.Subjects;

namespace Rzeka
{
    internal class SourceContract<T> : IDisposable  where T : TMatter 
    {
        readonly IObservable<T> _source;
        readonly Subject<T> _sourceSubject;

        public Subject<T> SourceSubject => _sourceSubject;

        IDisposable _sourceToken;

        public SourceContract(IObservable<T> source)
        {
            _source = source;
            _sourceSubject = new();
        }
        
        public void Begin()
        {
            _sourceToken = _source.Subscribe(next => _sourceSubject.OnNext(next));
        }

        public void Dispose()
        {
            SourceSubject?.Dispose();
            _sourceToken?.Dispose();
        }
    }
}