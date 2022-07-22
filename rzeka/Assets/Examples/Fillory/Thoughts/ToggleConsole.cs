/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using RzekaRiver;

namespace Examples.Fillory
{
    public class ConsoleOpenStatus : Matter
    {
        public bool IsConsoleOpen { get; set; }
    }

    public class ToggleConsole :
            Thought     <ConsoleOpenStatus>,
            HasDefault  <ToggleConsole, ConsoleOpenStatus>
    {

        public override string Description
        {
            get
            {
                if (this.Matter.IsConsoleOpen == true)
                    return "The console is being opened.";
                else
                    return "The console is being closed.";
            }
        }

        public ConsoleOpenStatus Default => 
            new ConsoleOpenStatus { IsConsoleOpen = false };

    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */