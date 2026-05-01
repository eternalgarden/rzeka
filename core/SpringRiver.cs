/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Rzeka.Tests")]
[assembly: InternalsVisibleTo("Rzeka.Dev")]

namespace Rzeka;
public class SpringRiver : IRzeka, IDisposable
{
    // TODO Eris was made internal, this will require reworking legacy Consultate code
    // in unity sanctuary project, it needs to be moved here anyway.
    internal Eris Eris { get; }
    internal Library Library { get; }
    internal IWhisper Whispers { get; }

    internal SpringRiver(string name, RzekaRole role = RzekaRole.Local)
    {
        Eris = new Eris(name, role);
        Library = new(Eris);
        Whispers = new Whispers(Eris);
    }

    public void Dispose()
    {
        Library.Dispose(); // Library before Eris since Library's subscription is on Eris's stream.
        Eris.Dispose();
    }

    /* 🐋🐳 */

    #region IWhisper

    public void Whisper(string message, params IMatter[] circumstances)
    {
        Whispers.Whisper(message, circumstances);
    }

    public void Whisper(
        string message,
        RzekaMessageType rzekaMessageType,
        params IMatter[] circumstances
    )
    {
        Whispers.Whisper(message, rzekaMessageType, circumstances);
    }

    public void Whisper(Exception exception, params IMatter[] circumstances)
    {
        Whispers.Whisper(exception, circumstances);
    }

    public void Whisper(string message, Exception exception, params IMatter[] circumstances)
    {
        Whispers.Whisper(message, exception, circumstances);
    }

    #endregion // END IWhisper

    #region IRzeka

    public IObservable<T> Scry<T>()
        where T : IMatter => Library.GetConjurer<T>();

    public IDisposable Strand<TOut>(object who, IObservable<TOut> spell)
        where TOut : IMatter
    {
        StrandingSpell<TOut> newScroll = new StrandingSpell<TOut>(who, spell, Library, Eris);

        return Disposable.Create(() => newScroll.Dispose());
    }

    public void Pluck<T>(object who, T matter)
        where T : IMatter
    {
        Library.RegisterConjurer(Observable.Return(matter)).Dispose();
    }

    public IDisposable Shuttle<TIn, TOut>(object who, Func<IObservable<TIn>, IObservable<TOut>> spell)
        where TIn : IRequest
        where TOut : IResponse<TIn>
    {
        ShuttleSpell<TIn, TOut> newScroll = new ShuttleSpell<TIn, TOut>(who, spell, Library, Eris);
        return Disposable.Create(() => newScroll.Dispose());
    }

    public IDisposable Weave<T>(object who, IObserver<T> spell)
        where T : IMatter
    {
        AlteringScroll<T> newScroll = new AlteringScroll<T>(who, spell, Library, Eris);
        return Disposable.Create(() => newScroll.Dispose());
    }

    public IDisposable Weave<T1>(object who, Func<IObservable<T1>, IDisposable> spell)
        where T1 : IMatter
    {
        WeavingSpell1<T1> newSpell = new WeavingSpell1<T1>(who, spell, Eris, Library);
        newSpell.Cast();
        return Disposable.Create(newSpell.Dispose);
    }

    public IDisposable Weave<T1, T2>(
        object who,
        Func<IObservable<T1>, IObservable<T2>, IDisposable> spell
    )
        where T1 : IMatter
        where T2 : IMatter
    {
        WeavingSpell2<T1, T2> newSpell = new WeavingSpell2<T1, T2>(who, spell, Eris, Library);
        newSpell.Cast();
        return Disposable.Create(newSpell.Dispose);
    }

    public IDisposable Weave<T1, T2, T3>(
        object who,
        Func<IObservable<T1>, IObservable<T2>, IObservable<T3>, IDisposable> spell
    )
        where T1 : IMatter
        where T2 : IMatter
        where T3 : IMatter
    {
        WeavingSpell3<T1, T2, T3> newSpell = new WeavingSpell3<T1, T2, T3>(
            who,
            spell,
            Eris,
            Library
        );
        newSpell.Cast();
        return Disposable.Create(newSpell.Dispose);
    }

    public IDisposable Loom<T1, TOut>(
        object who,
        Func<IObservable<T1>, IObservable<TOut>> spell
    )
        where T1 : IMatter
        where TOut : IMatter
    {
        LoomingSpell1<T1, TOut> newScroll = new LoomingSpell1<T1, TOut>(
            who,
            spell,
            Library,
            Eris
        );

        return Disposable.Create(() => newScroll.Dispose());
    }

    public IDisposable Loom<T1, T2, TOut>(
        object who,
        Func<IObservable<T1>, IObservable<T2>, IObservable<TOut>> spell
    )
        where T1 : IMatter
        where T2 : IMatter
        where TOut : IMatter
    {
        LoomingSpell2<T1, T2, TOut> newScroll = new LoomingSpell2<T1, T2, TOut>(
            who,
            spell,
            Library,
            Eris
        );
        return Disposable.Create(() => newScroll.Dispose());
    }

    public IDisposable Loom<T1, T2, T3, TOut>(
        object who,
        Func<IObservable<T1>, IObservable<T2>, IObservable<T3>, IObservable<TOut>> spell
    )
        where T1 : IMatter
        where T2 : IMatter
        where T3 : IMatter
        where TOut : IMatter
    {
        LoomingSpell3<T1, T2, T3, TOut> newScroll = new LoomingSpell3<T1, T2, T3, TOut>(
            who,
            spell,
            Library,
            Eris
        );
        return Disposable.Create(() => newScroll.Dispose());
    }

    #endregion // END IRzeka

    /* 🐋🐳 */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 06 November 2022 🌊 */
