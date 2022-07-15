/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/

namespace Modules.EventStream
{
    public interface IStreamCartographerUI
    {
        // -------------

        void DrawNewEventTree(EventTree eventTree);
        void DrawNewChildTree(EventTree childTree);
        void DrawNewEventReceiver(EventTree eventTree, object receiver);
        void DrawnNewEventLog(EventLog eventLog);
    
        // -------------
    }
}
/* maria aurelia at 23 September 2021 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */