using System.Collections.Generic;
using System;

namespace Modules.EventStream
{
    public class EventTree
    {
        // -------------

        //
        // ──────────────────────────────────────────────────── I ──────────
        //   :::::: F I E L D S : :  :   :    :     :        :          :
        // ──────────────────────────────────────────────────────────────
        //


        public readonly UInt64 ID;
        public readonly StreamEvent streamEvent;
        public readonly bool isRoot;
        public readonly EventTree parentTree;

        List<EventTree> _childrenTrees;
        List<object> _eventReceivers;


        //
        // ──────────────────────────────────────────────────────────── II ──────────
        //   :::::: P R O P E R T I E S : :  :   :    :     :        :          :
        // ──────────────────────────────────────────────────────────────────────
        //


        public List<object> EventReceivers
        {
            // -------------

            get
            {
                if (_eventReceivers is null)
                {
                    _eventReceivers = new List<object>();
                }
                return _eventReceivers;
            }

            // -------------
        }

        public List<EventTree> ChildrenTrees
        {
            // -------------

            get
            {
                if (_childrenTrees is null) _childrenTrees = new List<EventTree>();
                return _childrenTrees;
            }

            // -------------
        }


        //
        // ────────────────────────────────────────────────────────────── III ──────────
        //   :::::: C O N S T R U C T O R : :  :   :    :     :        :          :
        // ────────────────────────────────────────────────────────────────────────
        //


        public EventTree(UInt64 ID, StreamEvent thisEvent, EventTree parentTree = null, bool isRoot = false)
        {
            // -------------

            if (thisEvent is null)
            {
                throw new NullReferenceException("Cannot create a new event tree for a null stream event.");
            }

            this.ID = ID;
            this.streamEvent = thisEvent;
            this.isRoot = isRoot;
            this.parentTree = parentTree;

            // -------------
        }


        //
        // ──────────────────────────────────────────────────────────────────── IV ──────────
        //   :::::: P U B L I C   M E T H O D S : :  :   :    :     :        :          :
        // ──────────────────────────────────────────────────────────────────────────────
        //


        public void PropagateOnTree(Action<EventTree> onEventTree)
        {
            // -------------

            onEventTree.Invoke(this);

            if (_childrenTrees != null)
            {
                foreach (var tree in _childrenTrees)
                {
                    onEventTree.Invoke(tree);
                }
            }

            // -------------
        }

        public void AddChildTree(EventTree childTree)
        {
            // -------------

            if (_childrenTrees is null) _childrenTrees = new List<EventTree>();
            if (_childrenTrees.Contains(childTree))
            {
                // Is this necessary?
                throw new Exception("Parent tree already contains this event tree as a child");
            }
            _childrenTrees.Add(childTree);

            // -------------
        }

        public EventTree GetRootTree()
        {
            // -------------
            
            if (isRoot == true)
            {
                return this;
            }
            else
            {
                return parentTree.GetRootTree();
            }
            
            // -------------
        }

        // -------------
    }
}