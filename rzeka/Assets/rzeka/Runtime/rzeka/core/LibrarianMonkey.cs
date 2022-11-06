/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    public enum ScrollCategory { Plucking, Looming, Weaving }

    public class Introduction
    {
        public ScrollCategory ScrollCategory { get; set; }
        public TScrollBase Scroll { get; set; }
    }

    public class SpringRiver
    {
        public LibrarianMonkey LibrarianMonkey { get; }
        public Eris Eris { get; }

        ISubject<Introduction> ObservableIntroductions { get; set; }

        public SpringRiver()
        {
            Eris = new Eris();
            LibrarianMonkey = new LibrarianMonkey();

            ObservableIntroductions = new Subject<Introduction>();
        }

        public IDisposable Pluck<T>(object who, IObservable<T> spell)
            where T : TMatter
        {
            ConjuringScroll<T> newScroll = LibrarianMonkey.CreateConjuring<T>(who, spell);
            
            // Tell others new conjuring has appeared
            // Conjuring is only interested in Giving
            var introduction = new Introduction
            {
                ScrollCategory = ScrollCategory.Plucking,
                Scroll = newScroll
            };

            ObservableIntroductions.OnNext(introduction);

            // Listen to other introductions if anyone needs it
            
            ObservableIntroductions
                .Where(i => i.ScrollCategory is ScrollCategory.Looming or ScrollCategory.Weaving)
                .Select(i => i.Scroll as TBindingScroll)
                .Subscribe(bindingScroll => {
                    // gift itself
                });

            return Disposable.Create(() =>
            {
                // Tell others a conjuring is about to dispell
                newScroll.Dispose();
            });
        }

        public IDisposable Weave<T>(object who, IObserver<T> spell) where T : TMatter
        {
            AlteringScroll<T> newScroll = LibrarianMonkey.CreateWeaving(who, spell);

            var introduction = new Introduction
            {
                ScrollCategory = ScrollCategory.Weaving,
                Scroll = newScroll
            };

            ObservableIntroductions.OnNext(introduction);

            // * if it's requirements aren't met - enter the library for proper introductions
            ObservableIntroductions
                .Where(i => i.ScrollCategory is ScrollCategory.Looming or ScrollCategory.Plucking)
                .Select(i => i.Scroll as TBindingScroll)
                .Subscribe(bindingScroll => {
                    // gift itself
                });

            if ((Scroll as TBindingScroll).IsCastable)
            {
                // TheLibrary.CastWeaving(Scroll);
            }
            else
            {
                // TheLibrary.SaveBlockedBinding(Scroll, wasJustCreated: true);
            }

            return Disposable.Create(() =>
            {
                // TheLibrary.ForgetWeavingScroll(Scroll);
                // Scroll.Dispose();
            });
        }

        public IDisposable Loom<T, Q>(object who, Func<IObservable<T>, IObservable<Q>> spell)
            where T : TMatter
            where Q : TMatter
        {
            LoomingScroll<Q> newScroll = LibrarianMonkey.CreateLoom(who, spell);

            // Tell others new conjuring has appeared
            // 
            // That then they can just talk to each other

            // TheLibrary.CheckBindingScrollRequirements(newScroll);

            if (newScroll.IsCastable)
            {
                // TheLibrary.CastLooming(newScroll, wasJustCreated: true);
            }
            else
            {
                // TheLibrary.SaveBlockedBinding(newScroll, wasJustCreated: true);
            }

            return Disposable.Create(() =>
            {
                // TheLibrary.ForgetLoomScroll<Q>(newScroll);
                newScroll.Dispose();
            });
        }
    }

    public class LibrarianMonkey
    {
        // -------------

        public ConjuringScroll<Q> CreateConjuring<Q>(object who, IObservable<Q> spell) where Q : TMatter
        {
            //  ConjuringScroll<T> Scroll = new(who, spell, TheLibrary, Eris);

            return null;
        }

        public LoomingScroll<Q> CreateLoom<T, Q>(object who, Func<IObservable<T>, IObservable<Q>> spell) where T : TMatter where Q : TMatter
        {
            // LoomingScroll_1<T, Q> newScroll = new(who, spell, TheLibrary, Eris);

            return null;
        }

        public AlteringScroll<T> CreateWeaving<T>(object who, IObserver<T> spell) where T : TMatter
        {
            // LoomingScroll_1<T, Q> newScroll = new(who, spell, TheLibrary, Eris);

            return null;
        }

        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 06 November 2022 🌊 */