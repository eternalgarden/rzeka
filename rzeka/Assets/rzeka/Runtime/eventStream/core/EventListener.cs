/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/

using System;

namespace Modules.EventStream
{
    public class EventListener<T> where T : StreamEvent
    {
        // -------------

        public EventListener(object listener, Action<T> onEvent) 
        {
            Listener = listener;
            OnEvent = onEvent;
        }

        public object Listener { get; }
        public Action<T> OnEvent { get; }

        // -------------
    }
}
/* maria aurelia at 27 October 2021 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */