using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine;

namespace Rzeka
{
    /* ! Events for:
     
    - new pluck, loom, weaving
     */

    [Flags]
    public enum Meow
    {
        New = 1 << 0,
        Cast = 1 << 1,
        Blocked = 1 << 2,
    }

    public class StreamEvent
    {

    }

    public class MatterReceivedStreamEvent
    {
        //public TScrollBase 
    }


    public class Eris : IDisposable
    {
        EventHandler<TMatter> NextMatter { get; set; }
        EventHandler<Exception> NextException { get; }
        EventHandler NextCompletion { get; }

        EventHandler<TMatter> ReceivedMatter { get; set; }

        CollectibleDisposable _disposables;

        public Eris()
        {
            _disposables = new();

            _disposables += Observable.FromEventPattern<TMatter>(
                    h => NextMatter += h,
                    h => NextMatter -= h)
                .Select(pattern => (scroll: pattern.Sender as TScrollBase, matter: pattern.EventArgs))
                .Subscribe(o =>
                {
                    Debug.Log($"<color=cyan>RELEASE: <<{o.scroll.Who}>> received matter of type {o.matter.GetType()}</color>");
                });

            _disposables += Observable.FromEventPattern<TMatter>(
                    h => ReceivedMatter += h,
                    h => ReceivedMatter -= h)
                .Select(pattern => (scroll: pattern.Sender as TScrollBase, matter: pattern.EventArgs))
                .Subscribe(o =>
                {
                    Debug.Log($"<color=cyan>RECEIVE: <<{o.scroll.Who}>> received matter of type {o.matter.GetType()}</color>");
                });
        }

        public IObserver<T> GetReleasesObserver<T>(TScrollBase scroll) where T : TMatter
        {
            return Observer.Create<T>(
                onNext: val => NextMatter?.Invoke(scroll, val),
                onError: err => NextException?.Invoke(scroll, err),
                onCompleted: () => NextCompletion?.Invoke(scroll, null));
        }

        public IObserver<T> GetReceivalsObserver<T>(TScrollBase scroll) where T : TMatter
        {
            // todo extend with received exceptions and completions
            return Observer.Create<T>(
                onNext: val => ReceivedMatter.Invoke(scroll, val));
        }

        public void ScrollWasCreated(TScrollBase scroll)
        {
            Debug.Log($"<color=green>A new scroll was just created by {scroll.Who}, its type is: {scroll.GetType()}</color>");
        }

        public void ScrollWillBeCast(TScrollBase scroll, bool isNew = true)
        {

            string prefix = isNew ? "Just Created Scroll" : "Existing Scroll";
            Debug.Log($"<color=yellow>::{prefix}:: created by {scroll.Who} will be cast now, its type is: {scroll.GetType()}</color>");
        }

        public void ScrollWillBeBlocked(TScrollBase scroll, bool isNew)
        {
            string prefix = isNew ? "Just Created Scroll" : "Existing Scroll";
            Debug.Log($"<color=blue>::{prefix}:: created by {scroll.Who} is blocked, its type is: {scroll.GetType()}</color>");
        }

        public void ScrollWillBeDisposed(TScrollBase scroll)
        {
            Debug.Log($"<color=red>Scroll created by {scroll.Who} will be disposed, its type was: {scroll.GetType()}</color>");
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}