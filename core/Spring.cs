using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka;
public sealed class Spring
{
    readonly Subject<SpringRiver> _created = new();
    readonly Subject<SpringRiver> _disposed = new();
    bool _hasRiver;

    public IRzeka Create(
        string name,
        Action<ISpell, Exception>? onUnhandledSourceError = null,
        Func<object, string?>? describeOwner = null
    )
    {
        if (_hasRiver)
            throw new InvalidOperationException(
                "Spring already has a river. v1 supports a single river per Spring."
            );
        _hasRiver = true;
        var river = new SpringRiver(name, this);
        if (onUnhandledSourceError is not null)
            river.Eris.OnUnhandledSourceError = onUnhandledSourceError;
        if (describeOwner is not null)
            river.Eris.DescribeOwner = describeOwner;
        _created.OnNext(river);
        return river;
    }

    internal IObservable<SpringRiver> OnCreated => _created.AsObservable();
    internal IObservable<SpringRiver> OnDisposed => _disposed.AsObservable();

    internal void NotifyDisposed(SpringRiver river)
    {
        _hasRiver = false;
        _disposed.OnNext(river);
    }
}
