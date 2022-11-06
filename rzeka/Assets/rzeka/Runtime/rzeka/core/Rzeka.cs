using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Joins;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    public class RzekaXOXO : IRzeka
    {
        public TheLibrary TheLibrary { get; }
        public Eris Eris { get; }

        public RzekaXOXO()
        {
            Eris = new Eris();
            TheLibrary = new TheLibrary(Eris);
        }

        public IDisposable Pluck<T>(object who, IObservable<T> spell) 
            where T : TMatter
        {
            // ! $ NEW_PLUCK<Q>
            ConjuringScroll<T> Scroll = new(who, spell, TheLibrary, Eris);

            TheLibrary.CastConjuring(Scroll, wasJustCreated: true);

            return Disposable.Create(() =>
            {
                TheLibrary.ForgetConjuringScroll(Scroll);
                Scroll.Dispose();
            });
        }

        
        //
        // ⛺ ─── LOOMS ───────────────────────────────────────────────────
        //
        #region LOOMS
        
        public IDisposable Loom<T, Q>(object who, Func<IObservable<T>, IObservable<Q>> spell)
            where T : TMatter
            where Q : TMatter
        {
            LoomingScroll_1<T, Q> newScroll = new(who, spell, TheLibrary, Eris);

            TheLibrary.CheckBindingScrollRequirements(newScroll);

            if (newScroll.IsCastable)
            {
                TheLibrary.CastLooming(newScroll, wasJustCreated: true);
            }
            else
            {
                TheLibrary.SaveBlockedBinding(newScroll, wasJustCreated: true);
            }

            return Disposable.Create(() =>
            {
                TheLibrary.ForgetLoomScroll<Q>(newScroll);
                newScroll.Dispose();
            });
        }

        // TODO Write tests for Looms with multiple dependencies
        public IDisposable Loom<T, Y, Q>(object who, Func<IObservable<Glyph<T, Y>>, IObservable<Q>> spell)
            where T : TMatter
            where Y : TMatter
            where Q : TMatter
        {
            LoomingScroll_2<T, Y, Q> newScroll = new(who, spell, TheLibrary, Eris);

            TheLibrary.CheckBindingScrollRequirements(newScroll);

            if (newScroll.IsCastable)
            {
                TheLibrary.CastLooming(newScroll, wasJustCreated: true);
            }
            else
            {
                TheLibrary.SaveBlockedBinding(newScroll, wasJustCreated: true);
            }

            return Disposable.Create(() =>
            {
                TheLibrary.ForgetLoomScroll<Q>(newScroll);
                newScroll.Dispose();
            });
        }
        
        #endregion // ---------------------------------- LOOMS -------------------------

        public IDisposable Weave<T>(object who, IObserver<T> spell) where T : TMatter
        {
            // todo solve the missing concept of adding altering spells to _allKnownSpells as they cannot accept those at the moment
            // todo otherwise consider renaming or altogether burning down the all known spells library
            
            AlteringScroll<T> Scroll = new(who, spell, TheLibrary, Eris);

            TheLibrary.CheckBindingScrollRequirements(Scroll);

            if ((Scroll as TBindingScroll).IsCastable)
            {
                TheLibrary.CastWeaving(Scroll);
            }
            else
            {
                TheLibrary.SaveBlockedBinding(Scroll, wasJustCreated: true);
            }

            return Disposable.Create(() =>
            {
                TheLibrary.ForgetWeavingScroll(Scroll);
                Scroll.Dispose();
            });
        }

        public void Dispose()
        {
            Eris.Dispose();
        }
    }
}