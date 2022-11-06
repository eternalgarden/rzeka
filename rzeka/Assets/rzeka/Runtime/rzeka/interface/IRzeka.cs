using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Joins;
using UnityEngine;

namespace Rzeka
{
    public interface IRzeka : IDisposable
    {
        Eris Eris { get; } // todo not here
        TheLibrary TheLibrary { get; }
        IDisposable Pluck<Q>(object who, IObservable<Q> spell) where Q : TMatter;
        IDisposable Loom<T,Q>(object who, Func<IObservable<T>, IObservable<Q>> spell) where Q : TMatter where T : TMatter;
        IDisposable Loom<T,Y,Q>(object who, Func<IObservable<Glyph<T, Y>>, IObservable<Q>> spell) where Q : TMatter where T : TMatter where Y : TMatter;
        IDisposable Weave<T>(object who, IObserver<T> spell) where T : TMatter; // TODO add overload that lets you first filter
    }

    public interface IRzekaProposals
    {
        // ! Pure Givers 0-

        void Pluck<T>(object who, T newValue, params TMatter[] circumstances) where T : TMatter;
        // This one is to talk about since
        void Pluck<T>(object who, Func<T, T> update, params TMatter[] circumstances) where T : TMatter;


        // Here circumstances must be provided manually inside IObservable definition

        // ! Pure Takers AB+

        IDisposable Weave<T>(object who, IObserver<T> taker) where T : TMatter;
        IDisposable Weave<T>(object who, Func<IObservable<T>, IDisposable> takerSpell) where T : TMatter;
        IDisposable Weave<T, Y>(object who, Func<Pattern<T, Y>, IDisposable> takerSpell) where T : TMatter;

        // ! Contractors
        // So far I see them as dealing with Request / Response type of events

        // Answering
        IDisposable Answer<Tin, Tout>(object who, Func<Tin, IObservable<Tout>> onQuestion) where Tin : IRequest where Tout : IResponse<Tin>;
        IDisposable Answer<Tin, Tout>(object who, Func<IObservable<Tin>, IObservable<Tout>> onQuestion) where Tin : IRequest where Tout : IResponse<Tin>;

        // Requesting
        IDisposable Ask<Tin, Tout>(object who, Tin question, IObserver<Tout> answerObserver) where Tin : IRequest where Tout : IResponse<Tin>;
        IDisposable Ask<Tin, Tout>(object who, IObservable<Tin> questionStream, IObserver<Tout> answerObserver) where Tin : IRequest where Tout : IResponse<Tin>;
        IDisposable Ask<Tin, Tout>(object who, IObservable<Tin> questionStream, Func<IObservable<Tout>, IDisposable> onAnswerStream) where Tin : IRequest where Tout : IResponse<Tin>;

        // Notice it could quickly get nasty this way, there should be another chaining option
        //[Obsolete] IDisposable Ask<Tin, Y, Tout>(object who, Func<IObservable<Y>, IObservable<Tin>> questionStream, Func<IObservable<Tout>> onAnswerStream);

        // ! Looms
        // They are actually close to givers than to the takers even though they do seem to "observe" other streams
        // But they don't really stop to subscribe, they transform different streams into a new one

        // They are also somehow contracts

        // In this cases it must be assumed that the user assignes proper circumstances to the elements in stream
        IDisposable Loom<T>(object who, Func<IObservable<T>> observable) where T : TMatter; // ! moved back to pluck
        IDisposable Loom<T, Tout>(object who, Func<IObservable<T>, IObservable<Tout>> spell);
        IDisposable Loom<T, Y, Tout>(object who, Func<Pattern<T, Y>, IObservable<Tout>> spell);
        IDisposable Loom<T, Y, U, Tout>(object who, Func<Pattern<T, Y, U>, IObservable<Tout>> spell);
        IDisposable Loom<T, Y, U, X, Tout>(object who, Func<Pattern<T, Y, U, X>, IObservable<Tout>> spell);
        // TODO And so on
    }
}
