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
    public class SpringRiver : ITestableRzeka
    {
        readonly Library _library;
        public Eris Eris { get; }

        Library ITestableRzeka.Library => _library;

        // TODO could these two and actually Library aswell be moved to eris

        public SpringRiver()
        {
            Eris = new Eris();
            _library = new(Eris);
        }

        public IDisposable Strand<TOut>(object who, IObservable<TOut> spell)
            where TOut : TMatter
        {
            ConjuringScroll<TOut> newScroll = new ConjuringScroll<TOut>(who, spell, _library, Eris);

            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Weave<T>(object who, IObserver<T> spell) where T : TMatter
        {
            AlteringScroll<T> newScroll = new AlteringScroll<T>(who, spell, _library, Eris);
            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Weave<T1>(object who, Action<IObservable<T1>> spell) where T1 : TMatter
        {
            WeavingSpell1<T1> newSpell = new WeavingSpell1<T1>(who, spell, Eris, _library);
            return Disposable.Create(newSpell.Dispose);
        }

        public IDisposable Weave<T1, T2>(object who, Action<IObservable<Glyph<T1, T2>>> spell) where T1 : TMatter where T2 : TMatter
        {
            throw new NotImplementedException();
        }

        public IDisposable Weave<T1, T2, T3>(object who, Action<IObservable<Glyph<T1, T2, T3>>> spell) where T1 : TMatter where T2 : TMatter where T3 : TMatter
        {
            throw new NotImplementedException();
        }

        public IDisposable Loom<T1, TOut>(object who, Func<IObservable<T1>, IObservable<TOut>> spell)
            where T1 : TMatter
            where TOut : TMatter
        {
            LoomingScroll_1<T1, TOut> newScroll = new LoomingScroll_1<T1, TOut>(who, spell, _library, Eris);

            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Loom<T1, T2, TOut>(object who, Func<IObservable<Glyph<T1, T2>>, IObservable<TOut>> spell)
            where T1 : TMatter
            where T2 : TMatter
            where TOut : TMatter
        {
            LoomingScroll_2<T1, T2, TOut> newScroll = new LoomingScroll_2<T1, T2, TOut>(who, spell, _library, Eris);

            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Loom<T1, T2, T3, TOut>(object who, Func<IObservable<Glyph<T1, T2, T3>>, IObservable<TOut>> spell) where T1 : TMatter where T2 : TMatter where T3 : TMatter where TOut : TMatter
        {
            LoomingScroll_3<T1, T2, T3, TOut> newScroll = new LoomingScroll_3<T1, T2, T3, TOut>(who, spell, _library, Eris);
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
        
        public IDisposable Strand<Q>(object who, IObservable<Q> spell, out ConjuringScroll<Q> scroll)
            where Q : TMatter
        {
            ConjuringScroll<Q> newScroll = new ConjuringScroll<Q>(who, spell, _library, Eris);
            scroll = newScroll;
            return Disposable.Create(() => newScroll.Dispose());

        }

        public IDisposable Weave<T>(object who, IObserver<T> spell, out AlteringScroll<T> scroll) where T : TMatter
        {
            AlteringScroll<T> newScroll = new AlteringScroll<T>(who, spell, _library, Eris);
            scroll = newScroll;
            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Loom<T, Q>(object who, Func<IObservable<T>, IObservable<Q>> spell, out LoomingScroll_1<T, Q> scroll)
            where T : TMatter
            where Q : TMatter
        {
            LoomingScroll_1<T, Q> newScroll = new LoomingScroll_1<T, Q>(who, spell, _library, Eris);
            scroll = newScroll;
            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Loom<T, Y, Q>(object who, Func<IObservable<Glyph<T, Y>>, IObservable<Q>> spell, out LoomingScroll_2<T, Y, Q> scroll)
            where T : TMatter
            where Y : TMatter
            where Q : TMatter
        {
            LoomingScroll_2<T, Y, Q> newScroll = new LoomingScroll_2<T, Y, Q>(who, spell, _library, Eris);
            scroll = newScroll;
            return Disposable.Create(() => newScroll.Dispose());
        }
        
        #endregion // ---------------------------------- ITestableRzeka -------------------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 06 November 2022 🌊 */