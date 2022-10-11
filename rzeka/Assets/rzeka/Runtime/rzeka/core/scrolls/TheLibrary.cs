using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using UnityEngine;

namespace Rzeka
{
    public class TheLibrary
    {
        /*
         * Conjuring Scrolls dictionary may contain ConjuringScrolls and BindingScrolls
         * Blocked Scrolls dictionary may contain AlteringScrolls and BindingScrolls
         */
        readonly Dictionary<Type, List<IConjuringScroll>> _activeConjurings = new();
        readonly Dictionary<Type, List<TBindingScroll>> _activeBindings = new(); 
        readonly Dictionary<Type, List<TBindingScroll>> _blockedBindings = new();
        
        // TODO THIS ONE IS NEW REQUIRES TESTING

        Eris Eris { get; }

        public TheLibrary(Eris eris)
        {
            Eris = eris;
        }

        public bool IsConjurable<T>()
        {
            return _activeConjurings.ContainsKey(typeof(T));
        }

        public void CastConjuring<T>(TConjuringScroll<T> scroll) where T : TMatter
        {
            CastConjuring(typeof(T), scroll);
        }

        public void CastConjuring(Type castableScrollType, IConjuringScroll scroll)
        {
            if (scroll.IsCastable is false) throw new ArgumentException("scroll is not castable");

            // ! $ LIBRARY.NEW_CASTABLE_CONJURING<Q>

            // TODO add try catch
            scroll.Cast();

            SaveActiveConjuring(castableScrollType, scroll);
            UnblockScrolls(castableScrollType);
        }

        public void CastLooming(Type conjuredType, ILoomingScroll scroll)
        {
            if (scroll.IsCastable is false) throw new ArgumentException("scroll is not castable");

            scroll.Cast();
            
            SaveActiveConjuring(conjuredType, scroll);
            SaveActiveBinding(scroll);
        }
        
        public void CastWeaving(IAlteringScroll scroll)
        {
            if (scroll.IsCastable is false) throw new ArgumentException("scroll is not castable");

            // ! $ LIBRARY.NEW_CASTABLE_CONJURING<Q>
            // TODO make sure reverse folding exist
            
            // TODO add try catch
            scroll.Cast();

            SaveActiveBinding(scroll);
        }

        void SaveActiveConjuring(Type conjuredType, IConjuringScroll scroll)
        {
            // TODO make sure reverse folding exist

            if (_activeConjurings.ContainsKey(conjuredType) is false)
            {
                _activeConjurings[conjuredType] = new List<IConjuringScroll>();
            }
            
            // TODO UMMMM HANDLE MULTIPLE PROVIDERS OF SAME CONJURIJNG
            _activeConjurings[conjuredType].Add(scroll);
        }

        void SaveActiveBinding(TBindingScroll scroll)
        {
            for (var index = 0; index < scroll.Requirements.Length; index++)
            {
                var requirementType = scroll.Requirements[index];
                if (_activeBindings.ContainsKey(requirementType) is false)
                {
                    _activeBindings[requirementType] = new List<TBindingScroll>();
                }

                _activeBindings[requirementType].Add(scroll);
            }
        }

        void UnblockScrolls(Type unblockedType)
        {
            if (!_blockedBindings.ContainsKey(unblockedType)) return;

            List<TBindingScroll> unblockedScrolls = new(_blockedBindings[unblockedType].Count);
            foreach (TBindingScroll blockedScroll in _blockedBindings[unblockedType])
            {
                blockedScroll[unblockedType] = true;

                // todo at one time it might be better to differentiate between altering scrolls here who overload iscastable
                if (blockedScroll.IsCastable) unblockedScrolls.Add(blockedScroll);
            }

            UnblockScrolls(unblockedType, unblockedScrolls);
        }

        void UnblockScrolls(Type unblockedType, IEnumerable<TBindingScroll> scrolls)
        {
            foreach (TBindingScroll unblockedScroll in scrolls)
            {
                switch (unblockedScroll)
                {
                    case ILoomingScroll loomingScroll: // * These are the looming scrolls
                        // ! recurse
                        Eris.ScrollWillBeCast(loomingScroll, isNew: false);
                        CastLooming(loomingScroll.ConjuredType, loomingScroll);
                        break;
                    case IAlteringScroll alteringScroll:
                        Eris.ScrollWillBeCast(alteringScroll, isNew: false);
                        // todo this will be good to actually hold all scrolls reference to make sure there are no undisposed things
                        CastWeaving(alteringScroll);
                        break;
                    default:
                        throw new Exception("smth rlly wrong");
                }

                RemoveFromBlockedScrollsCollection(unblockedType, unblockedScroll);
            }
        }

        void RemoveScrollFromBlockedCollections(TBindingScroll unblockedScroll)
        {
            foreach (var kvp in unblockedScroll.AvailableIngredientsDictionary)
            {
                if (kvp.Value == false)
                {
                    RemoveFromBlockedScrollsCollection(kvp.Key, unblockedScroll);
                }
            }
        }

        // TODO LOOK AT USAGE IN ALTERING SCROLL, DISPOSING ISN'T PROPERLY HANDLED YET
        public void RemoveFromBlockedScrollsCollection(Type blockingType, TBindingScroll unblockedScroll)
        {
            // ! this can be 0, it is slightly inefficient if the scroll was unblocked by a given type before
            // todo guid check
            int removeCount = _blockedBindings[blockingType]
                .RemoveAll(scroll => scroll.Guid == unblockedScroll.Guid);

            if (removeCount > 1) throw new Exception("something weird happened");

            if (_blockedBindings[blockingType].Count == 0)
            {
                _blockedBindings.Remove(blockingType);
            }
        }

        public bool IsTypeBlockingSpells<T>(out TBindingScroll[] blockedScrolls)
        {
            if (_blockedBindings.ContainsKey(typeof(T)))
            {
                blockedScrolls = _blockedBindings[typeof(T)].ToArray();
                return true;
            }

            blockedScrolls = null;
            return false;
        }

        public void AddABlockedScroll(TBindingScroll scroll)
        {
            foreach (Type req in scroll.Requirements)
            {
                if (scroll[req] == false)
                {
                    if (_blockedBindings.ContainsKey(req) is false)
                    {
                        _blockedBindings[req] = new List<TBindingScroll>();
                    }

                    _blockedBindings[req].Add(scroll);
                }
            }
        }

        public void ForgetWeavingScroll(IAlteringScroll scroll)
        {
            
        }

        // todo rename this thing it is really misleading and doesnt seem to handle AlteringScrolls
        public void ForgetLoomScroll<Q>(TScrollBase scroll)
            where Q : TMatter
        {
            // ! $ LIBRARY.FORGETTING_A_SCROLL<Q>

            Type removedSpellType = scroll.GetType();

            if (scroll is ILoomingScroll<Q> looming)
            {
                // TODO THIS IS WRONG, SHOULD BE MORE LIKE WASCAST HERE
                if (looming.IsCastable) RemoveFromConjuringScrolls(looming);
                else RemoveScrollFromBlockedCollections(looming);
            }
            else
            {
                throw new NotImplementedException($"Typeof: {removedSpellType}");
            }
        }

        public void RemoveFromConjuringScrolls(IConjuringScroll scroll)
        {
            Type removedConjuring = scroll.ConjuredType;

            if (_activeConjurings[removedConjuring].RemoveAll(x => x.Guid == scroll.Guid) == 0)
                throw new Exception("Conjuring Scroll was not an active conjuring.");

            if (_activeConjurings[removedConjuring].Count == 0)
            {
                _activeConjurings.Remove(removedConjuring);
            }

            if (_activeBindings.ContainsKey(removedConjuring))
            {
                var newlyBlockedScrolls = _activeBindings[removedConjuring];
                
                foreach (TBindingScroll newlyBlocked in newlyBlockedScrolls)
                {
                    CheckBindingScrollRequirements(newlyBlocked);
                
                    // BUG!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    // TODO HANDLE NOMANA PROBLEMS
                    // LIKE YOU CANT JUST DISPOSE THEN SINCE IT WOULD DISPOSE ENTIRE SCROLL
                    // BUT THAT IS NOT WHAT IS WANTED
                
                    // TODO now add a check for existing blocked scrolls
                    AddABlockedScroll(newlyBlocked);

                    if (newlyBlocked is ILoomingScroll loomingScroll)
                    {
                        RemoveFromConjuringScrolls(loomingScroll); // ! recurse
                    }
                }
            }
        }

        public void AskForIngredient<T>(out IObservable<T> ingredient) where T : TMatter
        {
            Type type = typeof(T);

            if (!_activeConjurings.ContainsKey(type))
                throw new Exception("it shouldnt be possible since we checcked for iscastable");

            List<IConjuringScroll> scrolls = _activeConjurings[type];

            // TODO HANDLING MULTIPLE PROVIDERS OF A SAME MATTER TYPE
            if (scrolls.Count > 1) throw new NotImplementedException("multiple castable scrolls of same type");
            var conjuringScroll = scrolls[0] as TConjuringScroll<T>;

            Eris.ScrollWillBeCast(conjuringScroll, isNew: false);

            ingredient = conjuringScroll.GetConjuring();
        }

        public void CheckBindingScrollRequirements(TBindingScroll bindingScroll)
        {
            foreach (var req in bindingScroll.Requirements)
            {
                bindingScroll[req] = _activeConjurings.ContainsKey(req);
            }
        }
    }
}