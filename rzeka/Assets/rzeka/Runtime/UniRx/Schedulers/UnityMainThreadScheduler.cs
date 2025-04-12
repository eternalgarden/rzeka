using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using UnityEngine;

/*
 * Quick code with Chatty, bypasses all intelligent and thought out yet super complicated solutions
 * proposed by Unirx, one day maybe when time go to refactor unirx code and implement some more of it,
 * especially the microcoroutines seem super cute and neat.
 *
 * This Mono has to be added to something like global Project Installer in Zenject
 */
public class UnityMainThreadScheduler : MonoBehaviour, IScheduler
{
    static UnityMainThreadScheduler _instance;
    public static IScheduler Instance => _instance ? _instance : throw new Exception("UnityMainThreadScheduler not initialized.");

    readonly ConcurrentQueue<ScheduledItem> _actions = new ConcurrentQueue<ScheduledItem>();

    class ScheduledItem
    {
        public Action Action;
        public DateTimeOffset DueTime;
    }

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
    }

    public DateTimeOffset Now => DateTimeOffset.UtcNow;

    public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
    {
        var item = new ScheduledItem
        {
            Action = () => action(this, state),
            DueTime = Now
        };

        _actions.Enqueue(item);
        return Disposable.Empty;
    }

    public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        var item = new ScheduledItem
        {
            Action = () => action(this, state),
            DueTime = Now + dueTime
        };

        _actions.Enqueue(item);
        return Disposable.Empty;
    }

    public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime,
        Func<IScheduler, TState, IDisposable> action)
    {
        var item = new ScheduledItem
        {
            Action = () => action(this, state),
            DueTime = dueTime
        };

        _actions.Enqueue(item);
        return Disposable.Empty;
    }

    void Update()
    {
        var now = Now;
        while (_actions.TryPeek(out var item) && item.DueTime <= now)
        {
            _actions.TryDequeue(out var _);
            item.Action?.Invoke();
        }
    }
}