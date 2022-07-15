using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System;

namespace Modules.EventStream
{
    public class EventLogFormatter
    {
        // TODO
        // ? Shouldnt that be on the html renderer side?
    }

    public class StreamCartographer : IStreamCartographer
    {
        // -------------


        IStreamCartographerUI _cartographerUI;

        UInt64 IDCounter = 0;

        List<EventTree> rootEventTreeForest = new List<EventTree>(50);

        // TODO One day, consider event log creating garbage
        const int CHRONOLOGICAL_EVENT_LOG_CAPACITY = 100;
        Queue<EventLog> eventLogQueue = new Queue<EventLog>(CHRONOLOGICAL_EVENT_LOG_CAPACITY);

        IDisposable CartographerToken;

        public StreamCartographer(IStreamCartographerUI cartographerUI)
        {
            // -------------
            
            this._cartographerUI = cartographerUI;

            // -------------
        }

        public void LogEvent(EventLog log)
        {
            // -------------

            // Debug.Log($"<color=yellow>{log.LogType} : {log.EventType}</color>");

            switch (log.LogType)
            {
                case LogTypeEnum.Published:
                    OnNewEventInStream(log.StreamEvent);
                    break;
                case LogTypeEnum.Received:
                    OnEventReceived(log);
                    break;
                default:
                    // TODO Implement other event logging
                    break;
            }

            DisplayChronolgicallyEventLog(log);

            // -------------
        }

        // TODO rename?
        private void DisplayChronolgicallyEventLog(EventLog log)
        {
            // -------------

            _cartographerUI.DrawnNewEventLog(log);

            // -------------
        }

        void OnEventReceived(EventLog receivedEventLog)
        {
            // -------------

            object receiver = receivedEventLog.Context;

            if (TryFindParentEventTree(receivedEventLog.StreamEvent, out EventTree eventTree))
            {
                if (eventTree.EventReceivers.Contains(receiver))
                {
                    throw new Exception("Shouldnt be here at all oops");
                }
                else
                {
                    eventTree.EventReceivers.Add(receiver);
                    _cartographerUI.DrawNewEventReceiver(eventTree, receiver);
                }
            }
            else
            {
                throw new Exception("Shouldnt be here at all oops");
            }

            // -------------
        }

        void OnNewEventInStream(StreamEvent newEvent)
        {
            // -------------

            // TODO 0. Check if event type is on ignored list

            if (newEvent.ParentEvent != null &&
                TryFindParentEventTree(newEvent.ParentEvent, out EventTree parentTree))
            {
                EventTree newEventTree = new EventTree(IDCounter++, newEvent, parentTree, isRoot: false);
                parentTree.AddChildTree(newEventTree);
                _cartographerUI.DrawNewChildTree(newEventTree);
            }
            else
            {
                EventTree newEventTree = new EventTree(IDCounter++, newEvent, isRoot: true);
                rootEventTreeForest.Add(newEventTree);
                _cartographerUI.DrawNewEventTree(newEventTree);
            }

            // -------------
        }

        private bool SearchForTreeInARootForest(Func<EventTree, bool> treeFilter, out EventTree matchingEventTree)
        {
            // -------------

            matchingEventTree = null;
            bool foundEventTree = false;

            EventTree tempLambdaTree = null;

            for (int i = 0; i < rootEventTreeForest.Count; i++)
            {
                rootEventTreeForest[i].PropagateOnTree(tree =>
                {
                    if (treeFilter(tree))
                    {
                        foundEventTree = true;
                        tempLambdaTree = tree;
                    }
                });

                if (foundEventTree == true)
                {
                    matchingEventTree = tempLambdaTree;
                    return true;
                }
            }

            return false;

            // -------------
        }

        private bool TryFindParentEventTree(StreamEvent newEvent, out EventTree itsEventTree)
        {
            // -------------

            if (SearchForTreeInARootForest(tree => tree.streamEvent == newEvent, out itsEventTree))
            {
                return true;
            }
            else
            {
                // * Reconsider these checks, consider writing tests
                throw new Exception("that really shouldnt ever happen with current implementation");
            }

            // -------------
        }

        // -------------
    }
}
