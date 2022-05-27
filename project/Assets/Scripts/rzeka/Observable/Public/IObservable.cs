/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;

/*
now this is the question if I should define it

TODO for now I dont

there is a small difference in definition with out T
compared to the system
*/
namespace Rzeka
{
    #if !(NETFX_CORE || NET_4_6 || NET_STANDARD_2_0 || UNITY_WSA_10_0)

    // public interface IObservable<T>
    // {
    //     /* 🌊 ---- ---- */

    //     IDisposable Subscribe(IObserver<T> observer);

    //     /* ---- ---- ⛺ */
    // }
    
    #endif
}
/* maria aurelia at 25 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */