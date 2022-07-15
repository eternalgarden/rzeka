    /* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System.Collections.Generic;
using System;
using Zenject;
using UnityEngine;

namespace Modules.EventStream
{
    public class SanctuaryEventStream : MonoBehaviour, IEventStream
    {
        //
        // ──────────────────────────────────────────────────────────────────────────── nul ──────────
        //   :::::: N E S T E D   D E F I N I T I O N S : :  :   :    :     :        :          :
        // ──────────────────────────────────────────────────────────────────────────────────────
        //


        // ? Nice short artice on covariance and contravariance
        // ? https://www.hexacta.com/advanced-generics-language/
        class ListenersDictionary
        {
            private Dictionary<Type, List<object>> _listeners = new Dictionary<Type, List<object>>();

            public void AddListenerOfType<T>(EventListener<T> value) where T : StreamEvent
            {
                // -------------
                
                if (_listeners.ContainsKey(typeof(T)) == false)
                {
                    _listeners.Add(typeof(T), new List<object>());
                }

                _listeners[typeof(T)].Add(value);
                
                // -------------
            }

            public bool HasListenersOfType<T>() where T : StreamEvent
            {
                // -------------
                
                return _listeners.ContainsKey(typeof(T));
                
                // -------------
            }

            public void OnEachListenerOfType<T>(Action<EventListener<T>> onEachListener) where T : StreamEvent
            {
                // -------------
                
                var listeners = _listeners[typeof(T)];

                for (int i = 0; i < listeners.Count; i++)
                {
                    EventListener<T> listener = listeners[i] as EventListener<T>;
                    onEachListener.Invoke(listener);
                }
                
                // -------------
            }

            public void RemoveListenerOfType<T>(EventListener<T> listener) where T : StreamEvent
            {
                // -------------
                
                if (_listeners[typeof(T)].Contains(listener))
                {
                    _listeners[typeof(T)].Remove(listener);

                    if (_listeners[typeof(T)].Count == 0)
                    {
                        _listeners.Remove(typeof(T));
                    }
                }
                else
                {
                    throw new Exception($"Trying to unregister a listener <that is NOT registered> for an event of type: {typeof(T)}");
                }
                
                // -------------
            }
        }

        //
        // ──────────────────────────────────────────────────── I ──────────
        //   :::::: F I E L D S : :  :   :    :     :        :          :
        // ──────────────────────────────────────────────────────────────
        //


        [Inject] public IStreamCartographer _cartographer;

        IEventFactory _eventFactory;

        private readonly ListenersDictionary _listeners = new ListenersDictionary();

        // Properties

        public IEventFactory Factory => _eventFactory;

        public SanctuaryEventStream()
        {
            _eventFactory = new EventFactory(this);
        }

        //
        // ──────────────────────────────────────────────────────────────────────────────────── III ──────────
        //   :::::: E V E N T   S T R E A M   I N T E R F A C E : :  :   :    :     :        :          :
        // ──────────────────────────────────────────────────────────────────────────────────────────────
        //


        public IDisposable RegisterListener<T>(Action<T> onNext, object context) where T : StreamEvent
        {
            // -------------

            if (onNext is null)
            {
                throw new ArgumentNullException(nameof(onNext));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            EventListener<T> listener = new EventListener<T>(context, onNext);

            if (_listeners.HasListenersOfType<T>() == false)
            {
                _cartographer.LogEvent(new EventLog(LogTypeEnum.EventPoolOpened, listener.Listener, typeof(T)));
            }

            _cartographer.LogEvent(new EventLog(LogTypeEnum.Registered, listener.Listener, typeof(T)));

            _listeners.AddListenerOfType<T>(listener);

            return new RegistrationToken<T>(this, listener);

            // -------------
        }

        public void Publish<T>(object context, Action<T> onCreatedEvent = null, StreamEvent parentEvent = null) where T : StreamEvent, new()
        {
            // -------------

            T newEvent = Factory.CreateEvent<T>(context, onCreatedEvent, parentEvent);
            Publish<T>(newEvent);

            // -------------
        }

        public void Publish<T>(T nextEvent) where T : StreamEvent
        {
            // -------------

            if (nextEvent is null)
            {
                throw new ArgumentNullException(nameof(nextEvent));
            }

            _cartographer.LogEvent(new EventLog(LogTypeEnum.Published, nextEvent.Context, nextEvent));

            if (_listeners.HasListenersOfType<T>())
            {
                _listeners.OnEachListenerOfType<T>(listener =>
                {
                    _cartographer.LogEvent(new EventLog(LogTypeEnum.Received, listener.Listener, nextEvent));
                    listener.OnEvent(nextEvent);
                });
            }
            else
            {
                // TODO REPLACE WITH EVENT LOG FACTORY SO IT WILL BE POSSIBLE TO CYCLE THEM
                _cartographer.LogEvent(new EventLog(LogTypeEnum.NoListeners, nextEvent.Context, nextEvent));
                return;
            }

            // -------------
        }

        public void UnegisterListner<T>(EventListener<T> listener) where T : StreamEvent
        {
            // -------------

            if (listener is null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (_listeners.HasListenersOfType<T>() == false)
            {
                throw new Exception($"Trying to unregister a listener for unexisting event pool for event of type: {typeof(T)}");
            }
            else
            {
                _cartographer.LogEvent(new EventLog(LogTypeEnum.Unregistered, listener.Listener, typeof(T)));

                _listeners.RemoveListenerOfType<T>(listener);

                if (_listeners.HasListenersOfType<T>() == false)
                {
                    _cartographer.LogEvent(new EventLog(LogTypeEnum.EventPoolClosed, listener.Listener, typeof(T)));
                }
            }

            // -------------
        }
    }
}
/* maria aurelia at 27 October 2021 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */