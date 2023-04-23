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
using UnityEngine;

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
            StrandingSpell<TOut> newScroll = new StrandingSpell<TOut>(who, spell, _library, Eris);

            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Weave<T>(object who, IObserver<T> spell) where T : TMatter
        {
            AlteringScroll<T> newScroll = new AlteringScroll<T>(who, spell, _library, Eris);
            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Weave<T1>(object who, Func<IObservable<T1>,IDisposable> spell) where T1 : TMatter
        {
            WeavingSpell1<T1> newSpell = new WeavingSpell1<T1>(who, spell, Eris, _library);
            newSpell.Cast();
            return Disposable.Create(newSpell.Dispose);
        }

        public IDisposable Weave<T1, T2>(object who, Func<IObservable<Glyph<T1, T2>>,IDisposable> spell) where T1 : TMatter where T2 : TMatter
        {
            WeavingSpell2Glyph<T1, T2> newSpell = new WeavingSpell2Glyph<T1, T2>(who, spell, Eris, _library);
            newSpell.Cast();
            return Disposable.Create(newSpell.Dispose);
        }

        public IDisposable Weave<T1, T2, T3>(object who, Func<IObservable<Glyph<T1, T2, T3>>,IDisposable> spell) where T1 : TMatter where T2 : TMatter where T3 : TMatter
        {
            WeavingSpell3Glyph<T1,T2,T3> newSpell = new WeavingSpell3Glyph<T1,T2,T3>(who, spell, Eris, _library);
            newSpell.Cast();
            return Disposable.Create(newSpell.Dispose);
        }

        public IDisposable Loom<T1, TOut>(object who, Func<IObservable<T1>, IObservable<TOut>> spell)
            where T1 : TMatter
            where TOut : TMatter
        {
            LoomingSpell1<T1, TOut> newScroll = new LoomingSpell1<T1, TOut>(who, spell, _library, Eris);

            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Loom<T1, T2, TOut>(object who, Func<IObservable<Glyph<T1, T2>>, IObservable<TOut>> spell)
            where T1 : TMatter
            where T2 : TMatter
            where TOut : TMatter
        {
            LoomingSpell2<T1, T2, TOut> newScroll = new LoomingSpell2<T1, T2, TOut>(who, spell, _library, Eris);

            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Loom<T1, T2, T3, TOut>(object who, Func<IObservable<Glyph<T1, T2, T3>>, IObservable<TOut>> spell) where T1 : TMatter where T2 : TMatter where T3 : TMatter where TOut : TMatter
        {
            LoomingSpell3<T1, T2, T3, TOut> newScroll = new LoomingSpell3<T1, T2, T3, TOut>(who, spell, _library, Eris);
            return Disposable.Create(() => newScroll.Dispose());

        }

        public void Dispose()
        {
            Debug.Log($"<color=yellow>rzeka disposed</color>");
            Eris.Dispose();
        }

        
        //
        // ⛺ ─── ITestableRzeka ───────────────────────────────────────────────────
        //
        #region ITestableRzeka
        
        public IDisposable Strand<Q>(object who, IObservable<Q> spell, out StrandingSpell<Q> scroll)
            where Q : TMatter
        {
            StrandingSpell<Q> newScroll = new StrandingSpell<Q>(who, spell, _library, Eris);
            scroll = newScroll;
            return Disposable.Create(() => newScroll.Dispose());

        }

        public IDisposable Weave<T>(object who, IObserver<T> spell, out AlteringScroll<T> scroll) where T : TMatter
        {
            AlteringScroll<T> newScroll = new AlteringScroll<T>(who, spell, _library, Eris);
            scroll = newScroll;
            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Loom<T, Q>(object who, Func<IObservable<T>, IObservable<Q>> spell, out LoomingSpell1<T, Q> scroll)
            where T : TMatter
            where Q : TMatter
        {
            LoomingSpell1<T, Q> newScroll = new LoomingSpell1<T, Q>(who, spell, _library, Eris);
            scroll = newScroll;
            return Disposable.Create(() => newScroll.Dispose());
        }

        public IDisposable Loom<T, Y, Q>(object who, Func<IObservable<Glyph<T, Y>>, IObservable<Q>> spell, out LoomingSpell2<T, Y, Q> scroll)
            where T : TMatter
            where Y : TMatter
            where Q : TMatter
        {
            LoomingSpell2<T, Y, Q> newScroll = new LoomingSpell2<T, Y, Q>(who, spell, _library, Eris);
            scroll = newScroll;
            return Disposable.Create(() => newScroll.Dispose());
        }
        
        #endregion // ---------------------------------- ITestableRzeka -------------------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 06 November 2022 🌊 */