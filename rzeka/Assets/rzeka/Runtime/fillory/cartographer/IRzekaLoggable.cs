/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/

using System;

namespace RzekaRiver
{
    /* ---- ---- ⛺ */

    public enum RzekaLogType
    {
        Info,
        Warning,
        Error,
        RedAlert,
    }

    public class RzekaLogGift : Matter
    {
        public RzekaLogType Type { get; set; }
        public string Text { get; set; }
        public Exception Exception { get; set; }
    }

    public interface IRzekaLoggable
    {
        void Log(RzekaLogGift logGife);
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */