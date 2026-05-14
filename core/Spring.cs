using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka;
public sealed class Spring
{
    readonly object _lock = new();
    readonly List<SpringRiver> _instances = new();
    readonly Subject<SpringRiver> _created = new();
    readonly Subject<SpringRiver> _disposed = new();

    public IRzeka Create(string name, Action<ISpell, Exception>? onUnhandledSourceError = null)
    {
        var river = new SpringRiver(name, this);
        if (onUnhandledSourceError is not null)
            river.Eris.OnUnhandledSourceError = onUnhandledSourceError;
        lock (_lock) _instances.Add(river);
        _created.OnNext(river);
        return river;
    }

    internal IObservable<SpringRiver> OnCreated => _created.AsObservable();
    internal IObservable<SpringRiver> OnDisposed => _disposed.AsObservable();

    /// <summary>Snapshot of currently-alive rivers, then every future creation. Pair with OnDisposed.</summary>
    internal IObservable<SpringRiver> Watch() => Observable.Defer(() =>
    {
        SpringRiver[] snapshot;
        lock (_lock) snapshot = _instances.ToArray();
        return snapshot.ToObservable().Concat(_created);
    });

    internal void NotifyDisposed(SpringRiver river)
    {
        bool removed;
        lock (_lock) removed = _instances.Remove(river);
        if (removed) _disposed.OnNext(river);
    }
}
