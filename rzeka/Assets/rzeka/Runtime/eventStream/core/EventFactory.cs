using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace Modules.EventStream
{
    public abstract class IEventFactory
    {
        protected IEventStream EventStream { get; private set; }

        public IEventFactory(IEventStream eventStream)
        {
            EventStream = eventStream;
        }

        public abstract T CreateEvent<T>(
            object context,
            Action<T> onCreatedEvent = null,
            StreamEvent parentEvent = null)
            where T : StreamEvent, new();

        public abstract IDisposable RegisterPromise<T>(
            object context, 
            Action<T> onResolved, 
            Action<StreamErrorEvent> onError = null, 
            Action onReceived = null, 
            StreamEvent parentEvent = null) where T : PromiseEvent<T>, new();
        
        public abstract PromiseResolvingEvent<T> PublishPromiseResolveEvent<T>(
            PromiseEvent<T> promiseEvent,
            T resolution,
            object resolver);

        public abstract void PublishError(object context,
            StreamEvent parent,
            string errorMessage = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

    }

    public class EventFactory : IEventFactory
    {
        public EventFactory(IEventStream eventStream) : base(eventStream)
        {
        }

        public override T CreateEvent<T>(
            object context,
            Action<T> onCreatedEvent = null,
            StreamEvent parentEvent = null
        )
        {
            T newEvent = new T();
            newEvent.Context = context;
            newEvent.ParentEvent = parentEvent;
            onCreatedEvent?.Invoke(newEvent);
            return newEvent;
        }

        public override IDisposable RegisterPromise<T>(
            object context, 
            Action<T> onResolved, 
            Action<StreamErrorEvent> onError = null, 
            Action onReceived = null, 
            StreamEvent parentEvent = null)
        {
            var promise = new T();

            promise.Initialize(context, onResolved, onError, onReceived, parentEvent);

            return promise as IDisposable;
        }

        public override PromiseResolvingEvent<T> PublishPromiseResolveEvent<T>(
            PromiseEvent<T> promiseEvent,
            T resolution,
            object resolver)
        {
            /* ⭐ ---- ---- */

            var resolvingEvent = new PromiseResolvingEvent<T>(resolution);
            resolvingEvent.Context = resolver;
            resolvingEvent.ParentEvent = promiseEvent;

            EventStream.Publish(resolvingEvent);

            return resolvingEvent;
            
            /* ---- ---- 🌠 */
        }

        public override void PublishError(
            object context,
            StreamEvent parent,
            string errorMessage = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            StringBuilder sb = new StringBuilder(errorMessage);
            sb.AppendLine();
            sb.AppendLine($"member: {memberName}");
            sb.AppendLine($"line: {sourceLineNumber}");
            sb.AppendLine($"file path: {sourceFilePath}");

            StreamErrorEvent errorEvent = new StreamErrorEvent(sb.ToString());
            errorEvent.Context = context;
            errorEvent.ParentEvent = parent;

            EventStream.Publish<StreamErrorEvent>(errorEvent);
        }
    }
}
