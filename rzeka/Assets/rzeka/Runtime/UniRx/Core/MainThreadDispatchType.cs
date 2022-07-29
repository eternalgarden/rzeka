/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/

namespace Rzeka.Unirx
{
    public enum MainThreadDispatchType
    {
        Update,
        FixedUpdate,
        EndOfFrame,
        GameObjectUpdate,
        LateUpdate,
    }
}