/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rzeka.Stream;
using UnityEngine;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    public class LoggingCartographer : IRzekaChartable<StreamEvent>
    {
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(StreamEvent value)
        {
            Debug.Log($"<color=yellow>Event {value.GetType()} :: {value.Description}</color>");
        }

        public void OnSubscribed(IObserver<StreamEvent> observer)
        {
            // * this is dirty that I assumed that other type
            WhoObserver<StreamEvent> cat = observer as WhoObserver<StreamEvent>;

            if (cat.WhoAsGameObject(out object plain, out GameObject game))
            {
                Debug.Log($"<color=green>Game subscriber {game.name}</color>", game.transform);
            }
            else
            {
                Debug.Log($"<color=green>Intance subscriber {plain.GetType()}</color>");
            }
        }

        public void OnUnsubscribed(IObserver<StreamEvent> observer)
        {
            WhoObserver<StreamEvent> cat = observer as WhoObserver<StreamEvent>;

            if (cat.WhoAsGameObject(out object plain, out GameObject game))
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