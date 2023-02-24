using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Joins;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly:InternalsVisibleTo("com.rzeka.tests.playmode")]

namespace Rzeka
{
    public interface IRzeka : IDisposable
    {
        Eris Eris { get; } // TODO make Eris internal
        IDisposable Strand<TOut>(object who, IObservable<TOut> spell) where TOut : TMatter;
        IDisposable Loom<T1,TOut>(object who, Func<IObservable<T1>, IObservable<TOut>> spell) where TOut : TMatter where T1 : TMatter;
        IDisposable Loom<T1,T2,TOut>(object who, Func<IObservable<Glyph<T1, T2>>, IObservable<TOut>> spell) where TOut : TMatter where T1 : TMatter where T2 : TMatter;
        IDisposable Loom<T1,T2,T3,TOut>(object who, Func<IObservable<Glyph<T1, T2, T3>>, IObservable<TOut>> spell) where TOut : TMatter where T1 : TMatter where T2 : TMatter where T3 : TMatter;
        IDisposable Weave<T>(object who, IObserver<T> spell) where T : TMatter;
        IDisposable Weave<T1>(object who, Action<IObservable<T1>> spell) where T1 : TMatter;
        IDisposable Weave<T1,T2>(object who, Action<IObservable<Glyph<T1, T2>>> spell) where T1 : TMatter where T2 : TMatter;
        IDisposable Weave<T1,T2,T3>(object who, Action<IObservable<Glyph<T1, T2, T3>>> spell) where T1 : TMatter where T2 : TMatter where T3 : TMatter;
    }

    internal interface ITestableRzeka : IRzeka
    {
        Library Library { get; }
        IDisposable Strand<Q>(object who, IObservable<Q> spell, out ConjuringScroll<Q> scroll) where Q : TMatter;
        IDisposable Loom<T,Q>(object who, Func<IObservable<T>, IObservable<Q>> spell, out LoomingScroll_1<T,Q> scroll) where Q : TMatter where T : TMatter;
        IDisposable Loom<T,Y,Q>(object who, Func<IObservable<Glyph<T, Y>>, IObservable<Q>> spell, out LoomingScroll_2<T,Y,Q> scroll) where Q : TMatter where T : TMatter where Y : TMatter;
        IDisposable Weave<T>(object who, IObserver<T> spell, out AlteringScroll<T> scroll) where T : TMatter; // TODO add overload that lets you first filter
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
