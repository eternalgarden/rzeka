using System;
using System.Reactive.Disposables;

namespace Rzeka
{
    public class CollectibleDisposable : IDisposable
    {
        CompositeDisposable _composite;

        public void Add(IDisposable disposable)
        {
            _composite ??= new();
            _composite.Add(disposable);
        }

        public void Dispose()
        {
            _composite?.Dispose();
            _composite = null;
        }

        public static CollectibleDisposable operator +(CollectibleDisposable a, IDisposable b)
        {
            (a ??= new ()).Add(b);
            return a;
        }
    }
}
