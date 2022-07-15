/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/

using System;
using UnityEngine;
using Zenject;

namespace Modules.EventStream
{

    public abstract class StreamEvent : IDisposable
    {
        // -------------

        public virtual string Description { get; } = "Undescribed event";
        public object Context { get; set; }
        public StreamEvent ParentEvent { get; set; }

        public StreamEvent() { }

        public StreamEvent(object context, StreamEvent parentEvent)
        {
            Context = context;
            ParentEvent = parentEvent;
        }

        public virtual void Dispose() { }

        // -------------
    }

    /*

        The consideration remains, there is a large group of Events that *request* something.

        Request can end in: SUCCESS, FAILURE (so for each request event type there will be 3 actual events)
        + PROCESSING: a possibly 4th state informing that the request has been received

    */

    public class StreamErrorEvent : StreamEvent
    {
        /* ⭐ ---- ---- */

        public StreamErrorEvent(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; }

        /* ---- ---- 🌠 */
    }


    public abstract class PromiseEvent<T> : StreamEvent
    {
        /* ⭐ ---- ---- */

        [Inject] IEventStream eventStream;
        Action<T> onResolved;
        Action<StreamErrorEvent> onError;
        Action onReceived;
        IDisposable resolveToken;

        public override string Description { get; } = "Undefined Promise";

        public void Initialize(
            object context,
            Action<T> onResolved,
            Action<StreamErrorEvent> onError = null,
            Action onReceived = null,
            StreamEvent parentEvent = null)
        {
            this.Context = context;
            this.onResolved = onResolved;
            this.onError = onError;
            this.onReceived = onReceived;
            this.ParentEvent = parentEvent;

            if (eventStream is null) Debug.LogError($"oopsie");

            this.resolveToken = eventStream.RegisterListener<PromiseResolvingEvent<T>>(
                onNext: e => {
                    if (e.ParentEvent != this) return;
                    onResolved.Invoke(e.Resolution);
                },
                context: context
            );
        }

        public override void Dispose()
        { 
            resolveToken.Dispose();
        }

        public void Resolve(T resolution, object resolver)
        {
            PromiseResolvingEvent<T> resolvingEvent = eventStream.Factory
                .PublishPromiseResolveEvent(this, resolution, resolver);
        }

        public void Err(StreamErrorEvent errorEvent)
        {
            if (onError is null)
            {
                Debug.LogError(errorEvent.ErrorMessage);
            }
            else
            {
                onError.Invoke(errorEvent);
            }
        }

        public void Reseive()
        {
            if (onReceived is null)
            {
                Debug.Log($"Promise of type :: {this.GetType()} :: isn't ready to receive received notifications.");
            }
            else
            {
                onReceived.Invoke();
            }
        }

        /* ---- ---- 🌠 */
    }

    public class PromiseResolvingEvent<T> : StreamEvent
    {
        public T Resolution { get; private set; }

        public PromiseResolvingEvent(T resolution)
        {
            Resolution = resolution;
        }
    }
}
/* maria aurelia at 27 October 2021 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */