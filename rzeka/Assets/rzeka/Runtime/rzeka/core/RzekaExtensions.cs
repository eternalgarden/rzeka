/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Linq;

namespace Rzeka
{
    public static class RzekaExtensions
    {
        // -------------
    
        public static bool RespondsTo<TRequest, TResponse>(this TResponse response, TRequest request)
            where TRequest : IRequest
            where TResponse : IResponse<TRequest>
        {
            return response.Request.Guid == request.Guid;
        }
        
        public static T WithCircumstances<T>(this T matter, params TMatter[] circumstances) 
            where T : TMatter
        {
            matter.Circumstances.Clear();
            matter.Circumstances.AddRange(circumstances);
        
            return matter;
        }

        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 04 November 2022 🌊 */