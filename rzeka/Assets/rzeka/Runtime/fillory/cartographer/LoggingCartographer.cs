/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using UnityEngine;

namespace RzekaRiver
{
    /* 🌊 ---- ---- */

    [CreateAssetMenu(fileName = "DebugCartographer.asset", menuName = "rzeka/Debug Cartographer")]
    internal class DebugCartographer : RzekaCharter
    {
        public override void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public override void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public override void OnNext(StreamEvent value)
        {
            Debug.Log($"<color=yellow>Event {value.GetType()} :: {value.Description}</color>");
        }

        public override void OnObserved(RzekaObserver<StreamEvent> observer)
        {
            if (observer.WhoAsGameObject(out object plain, out GameObject game))
            {
                Debug.Log($"<color=green>Game subscriber {game.name}</color>", game.transform);
            }
            else
            {
                Debug.Log($"<color=green>Intance subscriber {plain.GetType()}</color>");
            }
        }

        public override void OnUnobserved(RzekaObserver<StreamEvent> observer)
        {
            if (observer.WhoAsGameObject(out object plain, out GameObject game))
            {
                Debug.Log($"<color=magenta>Game unsubscribed {game.name}</color>", game.transform);
            }
            else
            {
                Debug.Log($"<color=magenta>Intance unsubscribed {plain.GetType()}</color>");
            }
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 01 July 2022 🌊 */