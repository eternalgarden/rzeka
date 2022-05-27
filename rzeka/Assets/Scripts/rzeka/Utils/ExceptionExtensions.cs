/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;

namespace Rzeka
{
    public static class ExceptionExtensions
    {
        // -------------

        public static void Throw(this Exception exception)
        {
            // TODO Read up on why would this be useful
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
        }

        // -------------
    }
}