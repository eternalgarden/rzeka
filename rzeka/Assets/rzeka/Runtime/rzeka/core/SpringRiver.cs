/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka
{
    public class SpringRiver : IRzeka, ITestableRzeka
    {
        public LibrarianMonkey LibrarianMonkey { get; }
        public Eris Eris { get; }

        Subject<SpellOccurence> SpellStream { get; }
        Subject<MatterOccurence> MatterStream { get; }

        public SpringRiver()
        {
            SpellStream = new Subject<SpellOccurence>();
            MatterStream = new Subject<MatterOccurence>();

            Eris = new Eris(SpellStream.AsObservable(), MatterStream.AsObservable());
        }

        public IDisposable Pluck<Q>(object who, IObservable<Q> spell)
            where Q : TMatter
        {
            ConjuringScroll<Q> newScroll = new ConjuringScroll<Q>(who, spell, SpellStream, MatterStream);

            return Disposable.Create(() => newScroll.Dispose());

        }

        public IDisposable Weave<T>(object who, IObserver<T> spell) where T : TMatter
        {
            AlteringScroll<T> newScroll = new AlteringScroll<T>(who, spell, SpellStream, MatterStream);

            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Loom<T, Q>(object who, Func<IObservable<T>, IObservable<Q>> spell)
            where T : TMatter
            where Q : TMatter
        {
            LoomingScroll_1<T, Q> newScroll = new LoomingScroll_1<T, Q>(who, spell, SpellStream, MatterStream);

            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Loom<T, Y, Q>(object who, Func<IObservable<Glyph<T, Y>>, IObservable<Q>> spell)
            where T : TMatter
            where Y : TMatter
            where Q : TMatter
        {
            LoomingScroll_2<T, Y, Q> newScroll = new LoomingScroll_2<T, Y, Q>(who, spell, SpellStream, MatterStream);

            return Disposable.Create(() => newScroll.Dispose());
        }

        public void Dispose()
        {
            Eris.Dispose();
        }

        
        //
        // ⛺ ─── ITestableRzeka ───────────────────────────────────────────────────
        //
        #region ITestableRzeka
        
        public IDisposable Pluck<Q>(object who, IObservable<Q> spell, out ConjuringScroll<Q> scroll)
            where Q : TMatter
        {
            ConjuringScroll<Q> newScroll = new ConjuringScroll<Q>(who, spell, SpellStream, MatterStream);
            scroll = newScroll;
            return Disposable.Create(() => newScroll.Dispose());

        }

        public IDisposable Weave<T>(object who, IObserver<T> spell, out AlteringScroll<T> scroll) where T : TMatter
        {
            AlteringScroll<T> newScroll = new AlteringScroll<T>(who, spell, SpellStream, MatterStream);
            scroll = newScroll;
            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Loom<T, Q>(object who, Func<IObservable<T>, IObservable<Q>> spell, out LoomingScroll_1<T, Q> scroll)
            where T : TMatter
            where Q : TMatter
        {
            LoomingScroll_1<T, Q> newScroll = new LoomingScroll_1<T, Q>(who, spell, SpellStream, MatterStream);
            scroll = newScroll;
            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Loom<T, Y, Q>(object who, Func<IObservable<Glyph<T, Y>>, IObservable<Q>> spell, out LoomingScroll_2<T, Y, Q> scroll)
            where T : TMatter
            where Y : TMatter
            where Q : TMatter
        {
            LoomingScroll_2<T, Y, Q> newScroll = new LoomingScroll_2<T, Y, Q>(who, spell, SpellStream, MatterStream);
            scroll = newScroll;
            return Disposable.Create(() => newScroll.Dispose());
        }
        
        #endregion // ---------------------------------- ITestableRzeka -------------------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 06 November 2022 🌊 */