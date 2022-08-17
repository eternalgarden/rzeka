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

//namespace Examples.Fillory
//{
//    /* 🌊 ---- ---- */
    
//    class InMatter : Matter { }

//    class OutMatter : Matter { }

//    class PromisingThought : Thought<InMatter, OutMatter>
//    {
//        public PromisingThought(InMatter inputMatter) : base(inputMatter)
//        {
//        }

//        public override string Description => throw new NotImplementedException();
//    }

//    public class UnchartedGrounds : MonoBehaviour
//    {
//        [SerializeField] MonoRzeka rzeka;

//        void Awake()
//        {
//            // -------------

//            InMatter matter = new();

//            // IWeaveable<PromisingThought> promiseme = rzeka.Weave<PromisingThought, InMatter, OutMatter>(matter, this);

//            // promiseme.Subscribe()
            
//            // -------------
//        }
//    }
    
//    /* ---- ---- ⛺ */
//}
///* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
///* 22 July 2022 🌊 */