using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Rzeka.Tests")]

namespace Rzeka
{
    // TODO IDisposable should be hidden and related only directly to the instance
    public interface IRzeka : IRzekaLogFairy, IDisposable
    {
        Eris Eris { get; } // TODO make Eris internal
        IDisposable Strand<TOut>(object who, IObservable<TOut> spell) where TOut : TMatter;
        IDisposable Loom<T1, TOut>(object who, Func<IObservable<T1>, IObservable<TOut>> spell) where TOut : TMatter where T1 : TMatter;
        IDisposable Loom<T1, T2, TOut>(object who, Func<IObservable<T1>, IObservable<T2>, IObservable<TOut>> spell) where TOut : TMatter where T1 : TMatter where T2 : TMatter;
        IDisposable Loom<T1, T2, T3, TOut>(object who, Func<IObservable<T1>, IObservable<T2>, IObservable<T3>, IObservable<TOut>> spell) where TOut : TMatter where T1 : TMatter where T2 : TMatter where T3 : TMatter;
        IDisposable Weave<T>(object who, IObserver<T> spell) where T : TMatter;
        IDisposable Weave<T1>(object who, Func<IObservable<T1>, IDisposable> spell) where T1 : TMatter;
        IDisposable Weave<T1, T2>(object who, Func<IObservable<T1>, IObservable<T2>, IDisposable> spell) where T1 : TMatter where T2 : TMatter;
        IDisposable Weave<T1, T2, T3>(object who, Func<IObservable<T1>, IObservable<T2>, IObservable<T3>, IDisposable> spell) where T1 : TMatter where T2 : TMatter where T3 : TMatter;
    }

    internal interface ITestableRzeka : IRzeka
    {
        Library Library { get; }
        IDisposable Strand<Q>(object who, IObservable<Q> spell, out StrandingSpell<Q> scroll) where Q : TMatter;
        IDisposable Loom<T, Q>(object who, Func<IObservable<T>, IObservable<Q>> spell, out LoomingSpell1<T, Q> scroll) where Q : TMatter where T : TMatter;
        IDisposable Loom<T, Y, Q>(object who, Func<IObservable<T>, IObservable<Y>, IObservable<Q>> spell, out LoomingSpell2<T, Y, Q> scroll) where Q : TMatter where T : TMatter where Y : TMatter;
        IDisposable Weave<T>(object who, IObserver<T> spell, out AlteringScroll<T> scroll) where T : TMatter;
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

        // ! Contractors
        // So far I see them as dealing with Request / Response type of events

        // Answering
        IDisposable Answer<Tin, Tout>(object who, Func<Tin, IObservable<Tout>> onQuestion) where Tin : IRequest where Tout : IResponse<Tin>;
        IDisposable Answer<Tin, Tout>(object who, Func<IObservable<Tin>, IObservable<Tout>> onQuestion) where Tin : IRequest where Tout : IResponse<Tin>;

        // Requesting
        IDisposable Ask<Tin, Tout>(object who, Tin question, IObserver<Tout> answerObserver) where Tin : IRequest where Tout : IResponse<Tin>;
        IDisposable Ask<Tin, Tout>(object who, IObservable<Tin> questionStream, IObserver<Tout> answerObserver) where Tin : IRequest where Tout : IResponse<Tin>;
        IDisposable Ask<Tin, Tout>(object who, IObservable<Tin> questionStream, Func<IObservable<Tout>, IDisposable> onAnswerStream) where Tin : IRequest where Tout : IResponse<Tin>;

        // ! Looms
        IDisposable Loom<T>(object who, Func<IObservable<T>> observable) where T : TMatter;
        IDisposable Loom<T, Tout>(object who, Func<IObservable<T>, IObservable<Tout>> spell);
        IDisposable Loom<T, Y, Tout>(object who, Func<IObservable<T>, IObservable<Y>, IObservable<Tout>> spell);
        IDisposable Loom<T, Y, U, Tout>(object who, Func<IObservable<T>, IObservable<Y>, IObservable<U>, IObservable<Tout>> spell);
    }
}
