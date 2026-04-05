/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Collections;
using System.Linq;
using System.Reactive.Linq;

namespace Rzeka
{
    public static class RzekaExtensions
    {
        // -------------

        public static bool IsRespondingTo<TRequest, TResponse>(
            this TResponse response,
            TRequest request
        )
            where TRequest : IRequest
            where TResponse : IResponse<TRequest>
        {
            return response.Request.Guid == request.Guid;
        }

        public static bool IsCircumstancedBy<T, U>(this T matter, U other, int maxDepth = 3)
            where T : TMatter
            where U : TMatter
        {
            if (maxDepth <= 0)
                return false;
            if (other is null)
                return false;

            foreach (TMatter circumstance in matter.Circumstances)
            {
                if (other.Equals(circumstance))
                {
                    return true;
                }
            }

            foreach (TMatter circumstance in matter.Circumstances)
            {
                if (circumstance.IsCircumstancedBy(other, maxDepth - 1))
                {
                    return true;
                }
            }

            return false;
        }

        public static IObservable<T> Crossing<T>(this IObservable<T> source)
            where T : TMatter => source.Select(m => m.WithCircumstances<T>());

        public static IObservable<TOut> Ask<TIn, TOut>(this IRzeka rzeka, object who, TIn request)
            where TIn : IRequest
            where TOut : IResponse<TIn>
        {
            return Observable.Create<TOut>(observer =>
            {
                IDisposable subscription = rzeka
                    .Scry<TOut>()
                    .Where(r => r.IsRespondingTo(request))
                    .Subscribe(observer);

                rzeka.Pluck(who, request);

                return subscription;
            });
        }

        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 04 November 2022 🌊 */
