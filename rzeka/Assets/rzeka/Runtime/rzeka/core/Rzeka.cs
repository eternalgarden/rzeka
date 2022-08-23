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
        public TheLibrary Library { get; }
        public Eris Eris { get; }

        public RzekaXOXO()
        {
            Eris = new();
            Library = new(Eris);
        }

        public IDisposable Pluck<T>(object who, IObservable<T> spell) 
            where T : TMatter
        {
            // ! $ NEW_PLUCK<Q>
            ConjuringScroll<T> Scroll = new(who, spell, Library, Eris);
            Eris.ScrollWasCreated(Scroll);
            Eris.ScrollWillBeCast(Scroll);

            Type type = typeof(T);

            Library.AddConjuringScroll(type, Scroll);

            return Disposable.Create(() => Scroll.Dispose());
        }

        public IDisposable Loom<T, Q>(object who, Func<IObservable<T>, IObservable<Q>> spell)
            where T : TMatter
            where Q : TMatter
        {
            // ! $ NEW_LOOM<T,Q>
            LoomingScroll<T, Q> Scroll = new(who, spell, Library, Eris);
            Eris.ScrollWasCreated(Scroll);

            var bindingScroll = Scroll as TBindingScroll;
            Library.CheckBindingScrollRequirements(bindingScroll);

            if (bindingScroll.IsCastable)
            {
                // ! $ NEW_LOOM<T,Q>.CASTABLE
                Eris.ScrollWillBeCast(Scroll);

                Library.AddConjuringScroll(Scroll);
            }
            else
            {
                // ! so in most cases (?) you won't be able to stop a spell that has already been fully cast
                // ! however they will be only cast on specific demand, otherwise they will be kept as Scrolls

                // ! $ NEW_LOOM<T,Q>.BLOCKED
                Eris.ScrollWillBeBlocked(Scroll, isNew: true);

                Library.AddABlockedScroll(Scroll);

            }

            return Disposable.Create(() => Scroll.Dispose());
        }

        public IDisposable Weave<T>(object who, IObserver<T> spell) where T : TMatter
        {
            // todo solve the missing concept of adding altering spells to _allKnownSpells as they cannot accept those at the moment
            // todo otherwise consider renaming or altogether burning down the all known spells library
            
            // ! $ NEW_WEAVING<T>
            Type type = typeof(T);
            AlteringScroll<T> Scroll = new(who, spell, Library, Eris);

            Eris.ScrollWasCreated(Scroll);
            Library.CheckBindingScrollRequirements(Scroll);

            if ((Scroll as TBindingScroll).IsCastable)
            {
                // ! $ WEAVING<T>   .NEW.CAST - WHO..
                //Debug.Log("damn");
                Eris.ScrollWillBeCast(Scroll);
                Scroll.Cast(Library);
            }
            else
            {
                // ! $ WEAVING<T>   .NEW.BLOCKED
                //Debug.LogError("Blocked Cast Weave!");
                Eris.ScrollWillBeBlocked(Scroll, isNew: true);
                Library.AddABlockedScroll(Scroll);
            }

            return Disposable.Create(() => Scroll.Dispose());
        }

        public void Dispose()
        {
            Eris.Dispose();
        }
    }
}