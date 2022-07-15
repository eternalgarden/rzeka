/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RzekaRiver;
using TMPro;

namespace Examples.Fillory
{
    /* 🌊 ---- ---- */

    public class ConsoleText
    {
        public Color color = Color.white;
        public string Text;
    }
    
    public class GameConsole : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI consoleBodyTMP;

        void Start()
        {
            // -------------
            
            var strand = Rzeka
                .Strand<LogStrand>()
                .Observe(nextlog => {

                    string color = null;

                    switch(nextlog.Gift.Type)
                    {
                        case LogType.Info:
                            color = "white";
                            break;
                        case LogType.Warning:
                            color = "yellow";
                            break;
                        case LogType.Error:
                            color = "red";
                            break;
                        case LogType.RedAlert:
                            color = "magenta";
                            break;
                    }

                    consoleBodyTMP.text = $"<color={color}>{nextlog.Gift.Text}</color>\n{consoleBodyTMP.text}";

                }, this);
            
            // -------------
        }
    }
    
    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 06 July 2022 🌊 */