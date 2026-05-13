using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Diagnostics;
using System.Reflection;

namespace Rzeka;

public class Stream<T> : ISpellStream
    where T : IMatter
{
    public Type MatterType => typeof(T);

    IDisposable _sourcesSubscription;
    readonly ISubject<T> _subject;
    readonly IObserver<T> _subjectFeeder;
    readonly IObserver<T> _sourceObserver;

    bool _isStateful;
    int _activeSourceCount;

    public Stream()
    {
        _subject = CreateSubject();

        _subjectFeeder = Observer
            .Create<T>(onNext: next => _subject.OnNext(next))
            .NotifyOn(Scheduler.CurrentThread);

        // OnNext-only forwarder to the scheduled feeder. The real error boundary lives
        // upstream in each spell's CreateConjuring (.WhisperOnError on the conjurer),
        // so OnError isn't expected here. If one ever sneaks through (a non-spell
        // registration bypassing the boundary), Observer.Create's default OnError
        // rethrows on the source thread — loud, not silent.
        _sourceObserver = Observer.Create<T>(onNext: next => _subjectFeeder.OnNext(next));
    }

    ISubject<T> CreateSubject()
    {
        Type matterType = typeof(T);
        var attrs = matterType.GetCustomAttributes().ToArray();

        if (attrs.Any(attr => attr is HasStateAttribute))
        {
            _isStateful = true;
            return new ReplaySubject<T>(1, Scheduler.CurrentThread);
        }

        return new Subject<T>();
    }

    public IDisposable RegisterConjurer(IObservable<T> conjurer)
    {
        // Single-writer guard for [HasState] matter: only one active source allowed.
        // See CLAUDE.md "State evolution convention" and README "Evolving State".
        if (_isStateful && _activeSourceCount > 0)
        {
            throw new InvalidOperationException(
                $"{typeof(T).Name} is [HasState] and already has an active writer. "
                    + "Single-writer is required for stateful matter — dispose the existing "
                    + "writer before registering a new one."
            );
        }

        IDisposable token = conjurer.Subscribe(_sourceObserver);
        _activeSourceCount++;

        return Disposable.Create(() =>
        {
            token.Dispose();
            _activeSourceCount--;
        });
    }

    // * Notice Stream cannot force unregister those who already requested it as a source
    // * They need to do it on their own when they lose mana
    public IObservable<T> GetStreamAsObservable()
    {
        return _subject.AsObservable();
    }

    public void Dispose()
    {
        _sourcesSubscription?.Dispose();
        _subject.OnCompleted();
    }
}
