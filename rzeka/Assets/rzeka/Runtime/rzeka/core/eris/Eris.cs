using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rzeka
{
    /* ! Events for:
     
    - new pluck, loom, weaving
     */

    


    public class Eris : IDisposable
    {
        EventHandler<TMatter> NextMatter { get; set; }
        EventHandler<Exception> NextException { get; }
        EventHandler NextCompletion { get; }

        EventHandler<TMatter> ReceivedMatter { get; set; }

        Action<RealmEvent> OnRealmEvent { get; set; }

        CollectibleDisposable _disposables;

        public readonly IObservable<RealmEvent> RealmEventStream;

        public Eris()
        {
            _disposables = new CollectibleDisposable();

            BindMatterEvents();

            // RealmEventStream = Observable.FromEvent<RealmEvent>(
            //         h => OnRealmEvent += h,
            //         h => OnRealmEvent -= h)
            //     .Publish()
            //     .RefCount();
            
            // TODO CREATE "ANNOTATION" ATTRIBUTE
            // FOR PLACES THAT ARE UNCLEAR, NEED EXPLANATION, ARE CRYPTIC, BUT AT THE MOMENT IT
            // REALLY IS THE BEST SOLUTION WITHOUT OVERENGINEERING THINGS,
            // I ACTUALLY KIND OF LIKE CAPS LOCK, IM NOT SHOUTING
            // LIKE HERE ITS ABOUT .Replay(66)
            // It's just to make sure everything from the start that goes through Rzeka
            // can be recorded, 66 is a safe number for the amount of events that can
            // happen before Eris gets fully initialized, but it is indeed a 
            var connectableRealmEventStream = Observable
                .FromEvent<RealmEvent>(
                    h => OnRealmEvent += h,
                    h => OnRealmEvent -= h)
                .Replay(66);

            _disposables += connectableRealmEventStream.Connect();
            RealmEventStream = connectableRealmEventStream.AsObservable();
        }

        void BindMatterEvents()
        {
            _disposables += Observable.FromEventPattern<TMatter>(
                    h => NextMatter += h,
                    h => NextMatter -= h)
                .Select(pattern => (scroll: pattern.Sender as TScrollBase, matter: pattern.EventArgs))
                .Subscribe(o =>
                {
                    Debug.Log($"<color=green>pushing next matter {o.matter.Type.Name}</color>");
                    OnRealmEvent?.Invoke(new MatterEvent(o.matter, o.scroll, MatterEventType.Shaped));

                    // if (o.matter.Circumstances[0] != default)
                    // {
                    //     Print("cyan", $"{o.matter.GetType()} has circumstance!", $"guid: {o.matter.Circumstances[0]}");
                    // }
                    // Print("cyan", $"RELEASE::{o.matter.GetType().Name} ",
                    // $"by {FormatScroll(o.scroll)} at {o.scroll.Who.GetType().Name}");
                });

            _disposables += Observable.FromEventPattern<TMatter>(
                    h => ReceivedMatter += h,
                    h => ReceivedMatter -= h)
                .Select(pattern => (scroll: pattern.Sender as TScrollBase, matter: pattern.EventArgs))
                .Subscribe(o =>
                {
                    OnRealmEvent?.Invoke(new MatterEvent(o.matter, o.scroll, MatterEventType.Received));

                    // Print("green", $"RECEIVE::{o.matter.GetType().Name} ",
                        // $"by {FormatScroll(o.scroll)} at {o.scroll.Who.GetType().Name}");
                });
        }

        public void PushNextMatter(TScrollBase scroll, TMatter matter)
        {
            NextMatter?.Invoke(scroll, matter);
        }

        public IObserver<T> GetReleasesObserver<T>(TScrollBase scroll) where T : TMatter
        {
            return Observer.Create<T>(
                onNext: val => NextMatter?.Invoke(scroll, val),
                onError: err => NextException?.Invoke(scroll, err),
                onCompleted: () => NextCompletion?.Invoke(scroll, null));
        }

        public void PushReceivedMatter(TScrollBase scroll, TMatter matter)
        {
            ReceivedMatter?.Invoke(scroll, matter);
        }

        public IObserver<T> GetReceivalsObserver<T>(TScrollBase scroll) where T : TMatter
        {
            // todo extend with received exceptions and completions
            return Observer.Create<T>(
                onNext: val => ReceivedMatter.Invoke(scroll, val));
        }

        public void ScrollWillBeCast(TScrollBase scroll, bool isNew)
        {
            var flags = ScrollEventType.Cast;
            if (isNew) flags |= ScrollEventType.New;
            else flags |= ScrollEventType.Known;

            OnRealmEvent?.Invoke(new ScrollEvent(scroll, flags));

            string prefix = isNew ? "Just Created Scroll" : "Existing Scroll";
            // Print("white", $"CAST::{FormatScroll(scroll)} ", $"::{prefix}:: created by {scroll.Who.GetType().Name}");
        }

        public void ScrollWillBeBlocked(TScrollBase scroll, bool isNew)
        {
            ScrollEventType flags = ScrollEventType.Blocked;
            if (isNew) flags |= ScrollEventType.New;
            else flags |= ScrollEventType.Known;

            OnRealmEvent?.Invoke(new ScrollEvent(scroll, flags));

            string prefix = isNew ? "Just Created Scroll" : "Existing Scroll";
            // Print("yellow", $"BLOCKED::{FormatScroll(scroll)} ", $"::{prefix}:: created by {scroll.Who.GetType().Name}");
        }

        // ! could be forgotten new in case when the scroll is created but its introduction to rrzeka is rejected since it already has a provider of that type who doesnt accept multiple sources
        public void ScrollWillBeForgotten(TScrollBase scroll, bool isNew)
        {
            ScrollEventType flags = ScrollEventType.Blocked;
            if (isNew) flags |= ScrollEventType.New;
            else flags |= ScrollEventType.Known;
            
            OnRealmEvent?.Invoke(new ScrollEvent(scroll, flags));

            // Print("red", $"FORGOTTEN::{FormatScroll(scroll)} ", $"created by {scroll.Who.GetType().Name}");
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        void Print(string color, string head, string msg, params object[] args)
        {
            string text = string.Format(msg, args);
            // Debug.Log($"<color={color}>{head}</color><color=white>{text}</color>");
        }

        static string FormatScroll(TScrollBase scroll)
        {
            return scroll switch
            {
                IAlteringScroll alteration =>
                    $"Alteration<{alteration.Requirements.Aggregate("", (text, type) => text + $"{type.Name},")}>",
                TBindingScroll binding =>
                    $"Loom<{binding.Requirements.Aggregate("", (text, type) => text + $"{type.Name}")}>",
                IConjuringScroll conjuring => $"Conjuring<{conjuring.ConjuredType.Name}>",
                _ => throw new Exception("ungangled scroll type")
            };
        }
    }
}