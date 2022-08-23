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
        public TheLibrary Library { get; } = new();

        public IDisposable Pluck<T>(object who, IObservable<T> spell) 
            where T : TMatter
        {
            // ! $ NEW_PLUCK<Q>
            ConjuringScroll<T> Scroll = new() { spell = spell };

            Type type = typeof(T);

            Library.AddConjuringScroll(type, Scroll);

            return Disposable.Create(() => Library.RemoveFromConjuringScrolls(Scroll));
        }

        public IDisposable Loom<T, Q>(object who, Func<IObservable<T>, IObservable<Q>> spell)
            where T : TMatter
            where Q : TMatter
        {
            // ! $ NEW_LOOM<T,Q>
            LoomingScroll<T, Q> Scroll = new() { spell = spell };

            var bindingScroll = Scroll as TBindingScroll;
            Library.CheckBindingScrollRequirements(bindingScroll);

            if (bindingScroll.IsCastable)
            {
                // ! $ NEW_LOOM<T,Q>.CASTABLE
                Library.AddConjuringScroll(Scroll);
            }
            else
            {
                // ! so in most cases (?) you won't be able to stop a spell that has already been fully cast
                // ! however they will be only cast on specific demand, otherwise they will be kept as Scrolls

                // ! $ NEW_LOOM<T,Q>.BLOCKED
                Library.AddABlockedScroll(Scroll);

            }

            return Disposable.Create(() => Library.ForgetLoomScroll<Q>(Scroll));
        }

        public IDisposable Weave<T>(object who, IObserver<T> spell) where T : TMatter
        {
            // todo solve the missing concept of adding altering spells to _allKnownSpells as they cannot accept those at the moment
            // todo otherwise consider renaming or altogether burning down the all known spells library
            
            // ! $ NEW_WEAVING<T>
            Type type = typeof(T);
            AlteringScroll<T> Scroll = new() { spell = spell };

            Library.CheckBindingScrollRequirements(Scroll);

            if ((Scroll as TBindingScroll).IsCastable)
            {
                // ! $ NEW_WEAVING<T>.CAST
                //Debug.Log("damn");
                Scroll.Cast(Library);
            }
            else
            {
                // ! $ NEW_WEAVING<T>.BLOCKED
                //Debug.LogError("Blocked Cast Weave!");
                Library.AddABlockedScroll(Scroll);
            }

            return Disposable.Create(() =>
            {
                if (Scroll.WasCast) Scroll.Dispose();
                else Library.RemoveFromBlockedScrollsCollection(typeof(T),Scroll);
            });
        }
    }
}