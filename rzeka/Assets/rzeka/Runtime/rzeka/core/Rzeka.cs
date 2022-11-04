using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;
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

        public IDisposable Loom<T, Q>(object who, Func<IObservable<T>, IObservable<Q>> spell)
            where T : TMatter
            where Q : TMatter
        {
            // ! $ NEW_LOOM<T,Q>
            LoomingScroll_1<T, Q> Scroll = new(who, spell, TheLibrary, Eris);

            var bindingScroll = Scroll as TBindingScroll;
            TheLibrary.CheckBindingScrollRequirements(bindingScroll);

            if (bindingScroll.IsCastable)
            {
                TheLibrary.CastLooming(Scroll, wasJustCreated: true);
            }
            else
            {
                // ! so in most cases (?) you won't be able to stop a spell that has already been fully cast
                // ! however they will be only cast on specific demand, otherwise they will be kept as Scrolls

                TheLibrary.SaveBlockedBinding(Scroll, wasJustCreated: true);
            }

            return Disposable.Create(() =>
            {
                TheLibrary.ForgetLoomScroll<Q>(Scroll);
                Scroll.Dispose();
            });
        }

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