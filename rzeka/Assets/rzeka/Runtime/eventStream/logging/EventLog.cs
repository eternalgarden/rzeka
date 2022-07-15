/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;

namespace Modules.EventStream
{
    public class EventLog
    {
        // -------------

        readonly LogTypeEnum logtype;
        readonly object context;
        readonly Type eventType = null;
        readonly StreamEvent streamEvent = null;

        public LogTypeEnum LogType => logtype;

        public object Context => context;

        public Type EventType => eventType;

        public StreamEvent StreamEvent
        {
            get
            {
                if (streamEvent is null)
                {
                    throw new FieldAccessException($"Requested steam event object for an event log that does not contain one for eventType: {eventType.Name} of log type: {logtype}");
                }

                return streamEvent;
            }
        }


        //
        // ──────────────────────────────────────────────────────────────── II ──────────
        //   :::::: C O N S T R U C T O R S : :  :   :    :     :        :          :
        // ──────────────────────────────────────────────────────────────────────────
        //

        
        EventLog(LogTypeEnum logtype, object context)
        {
            this.logtype = logtype;
            this.context = context;
        }

        public EventLog(LogTypeEnum logtype, object context, StreamEvent streamEvent) : this(logtype, context)
        {
            this.streamEvent = streamEvent;
            eventType = streamEvent.GetType();
        }

        public EventLog(LogTypeEnum logtype, object context, Type eventType) : this(logtype, context)
        {
            this.eventType = eventType;
        }

        // -------------
    }
}
/* maria aurelia at 27 October 2021 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */