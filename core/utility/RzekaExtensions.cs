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

namespace Rzeka;
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
        // A null Request means the response was constructed wrong. ShuttleSpell whispers
        // a Horror about it; here we only refuse the match, so that one malformed response
        // won't explode any Ask that works on this Request/Response pair.
        return response.Request is not null && response.Request.Guid == request.Guid;
    }

    /// <summary>
    /// Stamp circumstances on a piece of matter. Returns a clone — the original is unchanged.
    /// Replaces any pre-existing circumstances; pass them all in one call if you need to merge.
    /// Mirrors <see cref="IMatter.WithCircumstances{T}"/> as an extension so concrete-typed
    /// call sites (e.g. <c>new GamePaused().WithCircumstances&lt;GamePaused&gt;(trigger)</c>) don't
    /// need an interface cast.
    /// </summary>
    public static T WithCircumstances<T>(this T matter, params IMatter[] circumstances)
        where T : IMatter
        => (T)matter.Clone(circumstances);

    public static bool IsCircumstancedBy<T, U>(this T matter, U other)
        where T : IMatter
        where U : IMatter
    {
        if (other is null)
            return false;

        return SearchCircumstances(matter, other, new HashSet<Guid>());
    }

    static bool SearchCircumstances(IMatter matter, IMatter other, HashSet<Guid> visited)
    {
        foreach (IMatter circumstance in matter.Circumstances)
        {
            if (!visited.Add(circumstance.Guid))
                continue;
            if (other.Equals(circumstance) || SearchCircumstances(circumstance, other, visited))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Dispatch a request and observe correlated responses.
    /// Subscribes a Weave to the response stream BEFORE plucking the request, so the registration
    /// is visible to Eris and there is no race. Caller controls cardinality (use <c>.Take(1)</c>
    /// for one-shot or omit for streaming). To thread a triggering matter as a circumstance on
    /// the request, pre-stamp via <c>request.WithCircumstances&lt;TIn&gt;(trigger)</c>.
    /// </summary>
    public static IObservable<TOut> Ask<TIn, TOut>(this IRzeka rzeka, object who, TIn request)
        where TIn : IRequest
        where TOut : IResponse<TIn>
    {
        return Observable.Create<TOut>(observer =>
        {
            IDisposable weave = rzeka.Weave<TOut>(
                who,
                src => src.Where(r => r.IsRespondingTo(request)).Subscribe(observer)
            );

            rzeka.Pluck(who, request);

            return weave;
        });
    }

    // -------------
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 04 November 2022 🌊 */
