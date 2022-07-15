/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;

namespace Modules.EventStream
{
    public class RegistrationToken<T> : IDisposable where T : StreamEvent
    {
        // -------------

        IEventStream eventStream;
        EventListener<T> listener;

        public RegistrationToken(IEventStream eventStream, EventListener<T> listener)
        {
            this.eventStream = eventStream;
            this.listener = listener;
        }

        public void Dispose()
        {
            eventStream.UnegisterListner<T>(listener);
        }

        // -------------
    }
}
/* maria aurelia at 27 October 2021 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */