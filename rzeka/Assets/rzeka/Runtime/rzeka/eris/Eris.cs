using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Rzeka
{
    public class Eris : IDisposable
    {
        // ⚠️ Pushes, currently not used, due to special operator requirement and possible high 
        EventHandler<TMatter> NextMatter { get; set; }
        EventHandler<Exception> NextException { get; }
        EventHandler NextCompletion { get; }

        // 🎣 Receivals
        EventHandler<TMatter> ReceivedMatter { get; set; }
        EventHandler<Exception> ReceivedException { get; set; }
        EventHandler ReceivedCompletion { get; set; }

        Action<RealmEvent> OnRealmEvent { get; set; }
        CollectibleDisposable _disposables;

        public readonly IObservable<RealmEvent> RealmEventStream;

        public Eris()
        {
            /* ⭐ ---- ---- */

            _disposables = new CollectibleDisposable();

            BindMatterEvents();
            RealmEventStream = BindRealmEvents();

            /* ---- ---- 🌠 */
        }

        private IObservable<RealmEvent> BindRealmEvents()
        {
            /* ⭐ ---- ---- */
            
            /*
             * Why? Whies?
             FOR PLACES THAT ARE UNCLEAR, NEED EXPLANATION, ARE CRYPTIC, BUT AT THE MOMENT IT
             REALLY IS THE BEST SOLUTION WITHOUT OVERENGINEERING THINGS,
             I ACTUALLY KIND OF LIKE CAPS LOCK, IM NOT SHOUTING
             LIKE HERE ITS ABOUT .Replay(66)
             It's just to make sure everything from the start that goes through Rzeka
             can be recorded, 66 is a safe number for the amount of events that can
             happen before Eris gets fully initialized, but it is indeed a 
             */
            var connectableRealmEventStream = Observable
                .FromEvent<RealmEvent>(
                    h => OnRealmEvent += h,
                    h => OnRealmEvent -= h)
                .Replay(66);

            _disposables += connectableRealmEventStream.Connect();

            return connectableRealmEventStream.AsObservable();
            
            /* ---- ---- 🌠 */
        }

        void BindMatterEvents()
        {
            // TODO temp disabled it not forever
            // _disposables += Observable.FromEventPattern<TMatter>(
            //         h => NextMatter += h,
            //         h => NextMatter -= h)
            //     .Select(pattern => (scroll: pattern.Sender as TScrollBase, matter: pattern.EventArgs))
            //     .Subscribe(o =>
            //     {
            //         OnRealmEvent?.Invoke(new MatterEvent(o.matter, o.scroll, MatterEventType.Shaped));
            //     });

            _disposables += Observable.FromEventPattern<TMatter>(
                    h => ReceivedMatter += h,
                    h => ReceivedMatter -= h)
                .Select(pattern => (scroll: pattern.Sender as TScrollBase, matter: pattern.EventArgs))
                .Subscribe(o =>
                {
                    OnRealmEvent?.Invoke(new MatterEvent(o.matter, o.scroll, MatterEventType.Received));
                });
        }

        
        //
        // ⛺ ─── Matter Event Calls ───────────────────────────────────────────────────
        //
        #region Matter Event Calls

        // Marked as obsolete and removed from usage due to bugs related with proper handling
        // of matter realeased by hot vs cold observables.
        // Received is just received, simpler to handle and rather enough.
        [Obsolete]
        public IObserver<T> GetReleasesObserver<T>(TScrollBase scroll) where T : TMatter
        {
            return Observer.Create<T>(
                onNext: val => NextMatter?.Invoke(scroll, val),
                onError: err => NextException?.Invoke(scroll, err),
                onCompleted: () => NextCompletion?.Invoke(scroll, null));
        }

        public IObserver<T> GetReceivalsObserver<T>(TScrollBase scroll) where T : TMatter
        {
            return Observer.Create<T>(
                onNext: val => ReceivedMatter?.Invoke(scroll, val),
                onError: err => ReceivedException?.Invoke(scroll, err),
                onCompleted: () => ReceivedCompletion?.Invoke(scroll, null));
        }
        
        #endregion // ---------------------------------- Matter Event Calls -------------------------

        
        //
        // ⛺ ─── Scroll Event Calls ───────────────────────────────────────────────────
        //
        #region Scroll Event Calls
        
        public void ScrollWillBeCast(TScrollBase scroll, bool isNew)
        {
            var flags = ScrollEventType.Cast;
            if (isNew) flags |= ScrollEventType.New;
            else flags |= ScrollEventType.Known;

            OnRealmEvent?.Invoke(new ScrollEvent(scroll, flags));
        }

        public void ScrollWillBeBlocked(TScrollBase scroll, bool isNew)
        {
            ScrollEventType flags = ScrollEventType.Blocked;
            if (isNew) flags |= ScrollEventType.New;
            else flags |= ScrollEventType.Known;

            OnRealmEvent?.Invoke(new ScrollEvent(scroll, flags));
        }

        // TODO could be forgotten new in case when the scroll is created but its 
        // introduction to rrzeka is rejected since it already has a provider of 
        // that type who doesnt accept multiple sources
        public void ScrollWillBeForgotten(TScrollBase scroll, bool isNew)
        {
            ScrollEventType flags = ScrollEventType.Blocked;
            if (isNew) flags |= ScrollEventType.New;
            else flags |= ScrollEventType.Known;

            OnRealmEvent?.Invoke(new ScrollEvent(scroll, flags));
        }
        
        #endregion // ---------------------------------- Scroll Event Calls -------------------------

        public void Dispose()
        {
            _disposables.Dispose();
        }

        void Print(string color, string head, string msg, params object[] args)
        {
            string text = string.Format(msg, args);
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