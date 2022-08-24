using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine;

namespace Rzeka
{
    /* ! Events for:
     
    - new pluck, loom, weaving
     */

    public enum ScrollEventType
    {
        Created,
        Cast,
        Blocked,
        Forgotten
    }

    public enum MatterEventType
    {
        Shaped,
        Received,
        Exception,
        Finished
    }

    public interface IRealmEvent
    {
    }

    public struct ScrollEvent : IRealmEvent
    {
        public ScrollEvent(TScrollBase scroll, ScrollEventType eventType, bool wasJustCreated = false)
        {
            Scroll = scroll;
            EventType = eventType;
            WasJustCreated = wasJustCreated;
        }

        public TScrollBase Scroll { get; }
        public ScrollEventType EventType { get; }
        public bool WasJustCreated { get; }
    }

    public struct MatterEvent : IRealmEvent
    {
        public MatterEvent(TMatter matter, TScrollBase source, MatterEventType eventType)
        {
            Matter = matter;
            Source = source;
            EventType = eventType;
        }

        public TMatter Matter { get; }
        public TScrollBase Source { get; }
        public MatterEventType EventType { get; }
    }


    public class Eris : IDisposable
    {
        EventHandler<TMatter> NextMatter { get; set; }
        EventHandler<Exception> NextException { get; }
        EventHandler NextCompletion { get; }

        EventHandler<TMatter> ReceivedMatter { get; set; }

        EventHandler<IRealmEvent> OnRealmEvent { get; set; }

        CollectibleDisposable _disposables;

        IObservable<IRealmEvent> _realmEventStream;

        public Eris()
        {
            _disposables = new();

            _disposables += Observable.FromEventPattern<TMatter>(
                    h => NextMatter += h,
                    h => NextMatter -= h)
                .Select(pattern => (scroll: pattern.Sender as TScrollBase, matter: pattern.EventArgs))
                .Subscribe(o =>
                {
                    Print("cyan", $"RELEASE::{o.matter.GetType().Name} ", $"by {FormatScroll(o.scroll)} at {o.scroll.Who.GetType().Name}");
                });

            _disposables += Observable.FromEventPattern<TMatter>(
                    h => ReceivedMatter += h,
                    h => ReceivedMatter -= h)
                .Select(pattern => (scroll: pattern.Sender as TScrollBase, matter: pattern.EventArgs))
                .Subscribe(o =>
                {
                    Print("green", $"RECEIVE::{o.matter.GetType().Name} ", $"by {FormatScroll(o.scroll)} at {o.scroll.Who.GetType().Name}");
                });

            _realmEventStream = Observable.FromEventPattern<IRealmEvent>(
                    h => OnRealmEvent += h,
                    h => OnRealmEvent -= h)
                .de;
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
            Print("white", $"CREATED::{FormatScroll(scroll)} ", $"by {scroll.Who.GetType().Name}");
            //Debug.Log($"<color=green>A new scroll was just created by {scroll.Who}, its type is: {scroll.GetType()}</color>");
        }

        public void ScrollWillBeCast(TScrollBase scroll, bool isNew = true)
        {
            string prefix = isNew ? "Just Created Scroll" : "Existing Scroll";
            Print("white", $"CAST::{FormatScroll(scroll)} ", $"::{prefix}:: created by {scroll.Who.GetType().Name}");
        }

        public void ScrollWillBeBlocked(TScrollBase scroll, bool isNew)
        {
            string prefix = isNew ? "Just Created Scroll" : "Existing Scroll";
            Print("yellow", $"BLOCKED::{FormatScroll(scroll)} ", $"::{prefix}:: created by {scroll.Who.GetType().Name}");
        }

        public void ScrollWillBeDisposed(TScrollBase scroll)
        {
            Print("red", $"FORGOTTEN::{FormatScroll(scroll)} ", $"created by {scroll.Who.GetType().Name}");
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        void Print(string color, string head, string msg, params object[] args)
        {
            string text = string.Format(msg, args);
            Debug.Log($"<color={color}>{head}</color><color=white>{text}</color>");
        }

        string FormatScroll(TScrollBase scroll)
        {
            if (scroll is TAlteringScroll alteration)
            {
                return $"Alteration<{alteration.Requirements.Aggregate("", (text, type) => text + $"{type.Name},")}>";
            }
            else if (scroll is TBindingScroll binding)
            {
                return $"Loom<{binding.Requirements.Aggregate("", (text, type) => text + $"{type.Name}")}>";
            }
            else if (scroll is IConjuringScroll conjuring)
            {
                return $"Conjuring<{conjuring.ConjuredType.Name}>";
            }
            else
            {
                throw new Exception("ungangled scroll type");
            }
        }
    }
}