using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka;
public sealed class Spring
{
    readonly Subject<SpringRiver> _created = new();
    readonly Subject<SpringRiver> _disposed = new();
    bool _hasRiver;

    // Creates the single river for this Spring.
    //
    // `mainThread` is the IScheduler representing the engine's main thread. All conjuring spells
    // (Strand, Loom, Shuttle) automatically ObserveOn this scheduler before publishing matter, so
    // matter publication is structurally guaranteed on the main thread - downstream subscribers
    // and the synchronous prefix of any downstream SelectMany lambda inherit main-thread context.
    // Pluck is intentionally NOT marshalled (it's a synchronous one-shot; queuing through a scheduler
    // would lose the emission when its token disposes); callers are expected to Pluck on main thread.
    //
    // For tests and non-engine contexts, pass ImmediateScheduler.Instance to keep emissions
    // synchronous on the current thread.
    //
    // Off-thread matter diagnostics are wired automatically: Spring.Create captures the calling
    // thread as the main thread and Eris whispers Horror if matter is ever published off it. Call
    // Spring.Create from your engine's main thread (Godot autoload _Ready, Unity startup, etc.).
    public IRzeka Create(
        string name,
        IScheduler mainThread,
        Action<ISpell, Exception>? onUnhandledSourceError = null,
        Func<object, string?>? describeOwner = null
    )
    {
        if (mainThread is null)
            throw new ArgumentNullException(nameof(mainThread));
        if (_hasRiver)
            throw new InvalidOperationException(
                "Spring already has a river. v1 supports a single river per Spring."
            );
        _hasRiver = true;
        var river = new SpringRiver(name, this);
        river.Eris.MainThread = mainThread;

        var mainThreadId = Environment.CurrentManagedThreadId;
        river.Eris.IsOnMainThread = () => Environment.CurrentManagedThreadId == mainThreadId;

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
