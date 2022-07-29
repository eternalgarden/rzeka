/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using Rzeka;
using UnityEngine;

namespace Examples.Fillory
{
    /* 🌊 ---- ---- */



    public class Logger
    {
        static Logger _instance;

        static Logger Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new Logger();
                }

                return _instance;
            }
        }

        public static void Log(RzekaLogType logType, string text, Exception ex = null)
        {
#if UNITY_EDITOR
            switch (logType)
            {
                case RzekaLogType.Info:
                    Debug.Log($"<color=white>{text}</color>");
                    break;
                case RzekaLogType.Warning:
                    Debug.Log($"<color=yellow>{text}</color>");
                    break;
                case RzekaLogType.Error:
                    Debug.LogError($"{text}");
                    break;
                case RzekaLogType.RedAlert:
                    Debug.LogError($"RED ALERT: {text}\nException Message: {ex.Message}\nStack trace: {ex.StackTrace}");
                    break;
            }
#endif

            // var log = new LogStrand();
            // log.Initialize(new LogGift()
            // {
            //     Type = logType,
            //     Text = text,
            //     Exception = ex
            // },
            //     context: _instance,
            //     Circumstances.UNKNOWN);

            // Rzeka.Pluck<LogStrand>(log);
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 06 July 2022 🌊 */