/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;

namespace Modules.EventStream
{
    [Obsolete] public interface IEventStream
    {
        // -------------

        IEventFactory Factory { get; }
        IDisposable RegisterListener<T>(Action<T> onNext, object context) where T : StreamEvent;
        void Publish<T>(T publishedEvent) where T : StreamEvent;
        void Publish<T>(object context, Action<T> onCreatedEvent = null, StreamEvent parentEvent = null) where T : StreamEvent, new();
        void UnegisterListner<T>(EventListener<T> listener) where T : StreamEvent;

        // -------------
    }

    public interface IEventStreamProposals : IEventStream
    {
        /* ? OPEN ISSUE
        Origin events provide information about the linear relation of causation however it does not contain information
        about the nature of 'contracts' that led them to make the decision to emit in response to them.

        + But that information is just subscription code they have

        Also they don't inform the stream on what happened when they just silently listen to the event.
        Did the event they received failed to meet criteria to continue?
        Or it simply completed it's own little 'stream' once receiving it, that it never actually plans to emit forther.

        + Concept on calling e.ThankYou(); on a received event that isn't continued from but just received and used/enjoyed.
        */

        /* WILD CONCEPT
        What if actually each event would be it's own stream with next, error, completed sequence.
        */

        /* PROPOSAL
        There is a possibility for a collection of the events to serve as origin event source isntead of a single parentEvent.
        */
        void Publish<T>(StreamEvent[] originEvents, object context, Action<T> onCreatedEvent = null) where T : StreamEvent, new();

        /* PROPOSAL
        Origin event CANNOT be null, if an event is considered a root then there should be a static instance of a root event or a static instanc eprovider.
        Com[ar UniRX for similar situation about empty observable.
        */
        void Publish<T>(StreamEvent originEvent, object context, Action<T> onCreatedEvent = null) where T : StreamEvent, new();

        /* PROPOSAL
        
        */
        void Promise<T>();
    }
}
/* maria aurelia at 27 October 2021 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */