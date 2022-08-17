///*
//      |\      _,,,---,,_
//ZZZzz /,`.-'`'    -.  ;-;;,_
//     |,4-  ) )-,_. ,\ (  `'-'
//    '---''(_/--'  `-'\_)

//most of the code straight out copied from @neuecc UniRx project
//https://github.com/neuecc/UniRx
//*/
//using System;
//using System.Linq;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Rzeka;
//using TMPro;
//using System.Reactive.Linq;
//using Rzeka.Unirx;

//namespace Examples.Fillory
//{
//    /* 🌊 ---- ---- */

//    public class ConsoleText
//    {
//        public Color color = Color.white;
//        public string Text;
//    }
    
//    public class GameConsole : MonoBehaviour
//    {
//        [SerializeField] MonoRzeka rzeka;
//        [SerializeField] TextMeshProUGUI consoleBodyTMP;

//        void Start()
//        {
//            // -------------
            
//            var strand = rzeka
//                .Weave<SpecialKeyPressed>(this)
//                .Subscribe(next => {
//                    WriteLine(next.Matter.Content.ToString());
//                });

//            // var strand2 = rzeka
//            //     .Weave<SpecialKeyPressed>(this)
//            //     .Where(key => key.Matter.Content == KeyCode.Tab)
//            //     .Subscribe(next => {
//            //         strand.Dispose();
//            //     });

//            var asdf = UnityObservable.EveryUpdate()
//                .Where(_ => {
//                    if (Input.GetKeyDown(KeyCode.Return))
//                    {
//                        return true;
//                    }
//                    else return false;
//                })
//                .Subscribe(keyCode =>
//                {
//                    strand.Dispose();
//                });
            
//            // -------------
//        }

//        public void WriteLine(string line, string color = "white")
//        {
//            consoleBodyTMP.text = $"<color={color}>{line}</color>\n{consoleBodyTMP.text}";
//        }   
//    }
    
//    /* ---- ---- ⛺ */
//}
///* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
///* 06 July 2022 🌊 */