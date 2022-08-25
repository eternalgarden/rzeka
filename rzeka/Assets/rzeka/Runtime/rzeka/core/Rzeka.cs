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
        public TheLibrary TheLibrary { get; set; }
        public Eris Eris { get; set; }

        public RzekaXOXO()
        {
            Eris = new();
            TheLibrary = new(Eris);
        }

        public IDisposable Pluck<T>(object who, IObservable<T> spell) 
            where T : TMatter
        {
            // ! $ NEW_PLUCK<Q>
            ConjuringScroll<T> Scroll = new(who, spell, TheLibrary, Eris);
            Eris.ScrollWillBeCast(Scroll, isNew: true);

            Type type = typeof(T);

            TheLibrary.AddConjuringScroll(type, Scroll);

            return Disposable.Create(() => Scroll.Dispose());
        }

        public IDisposable Loom<T, Q>(object who, Func<IObservable<T>, IObservable<Q>> spell)
            where T : TMatter
            where Q : TMatter
        {
            // ! $ NEW_LOOM<T,Q>
            LoomingScroll<T, Q> Scroll = new(who, spell, TheLibrary, Eris);

            var bindingScroll = Scroll as TBindingScroll;
            TheLibrary.CheckBindingScrollRequirements(bindingScroll);

            if (bindingScroll.IsCastable)
            {
                // ! $ NEW_LOOM<T,Q>.CASTABLE
                Eris.ScrollWillBeCast(Scroll, isNew: true);

                TheLibrary.AddConjuringScroll(Scroll);
            }
            else
            {
                // ! so in most cases (?) you won't be able to stop a spell that has already been fully cast
                // ! however they will be only cast on specific demand, otherwise they will be kept as Scrolls

                // ! $ NEW_LOOM<T,Q>.BLOCKED
                Eris.ScrollWillBeBlocked(Scroll, isNew: true);

                TheLibrary.AddABlockedScroll(Scroll);

            }

            return Disposable.Create(() => Scroll.Dispose());
        }

        public IDisposable Weave<T>(object who, IObserver<T> spell) where T : TMatter
        {
            // todo solve the missing concept of adding altering spells to _allKnownSpells as they cannot accept those at the moment
            // todo otherwise consider renaming or altogether burning down the all known spells library
            
            // ! $ NEW_WEAVING<T>
            Type type = typeof(T);
            AlteringScroll<T> Scroll = new(who, spell, TheLibrary, Eris);

            TheLibrary.CheckBindingScrollRequirements(Scroll);

            if ((Scroll as TBindingScroll).IsCastable)
            {
                // ! $ WEAVING<T>   .NEW.CAST - WHO..
                Eris.ScrollWillBeCast(Scroll, isNew: true);

                //Debug.Log("damn");
                Scroll.Cast(TheLibrary);
            }
            else
            {
                // ! $ WEAVING<T>   .NEW.BLOCKED
                //Debug.LogError("Blocked Cast Weave!");
                Eris.ScrollWillBeBlocked(Scroll, isNew: true);
                TheLibrary.AddABlockedScroll(Scroll);
            }

            return Disposable.Create(() => Scroll.Dispose());
        }

        public void Dispose()
        {
            Eris.Dispose();
        }
    }
}