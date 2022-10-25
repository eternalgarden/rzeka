using System;
using System.Collections.Generic;

namespace Rzeka
{
    public class TheLibrary
    {
        /*
         * 
         * 21.10.2022 CURRENT IMPLEMENTATION CONSIDERATIONS
         *
         * Current implementation aims at casting all scrolls ASAP.
         *
         * A scroll remains active as soon as it's requirements are met for as long as these
         * requirements remain satisfied or the scroll is disposed by its caster.
         *
         * TODO Consideration: Lazy instantiation
         * Non-weaving scrolls are not cast untill the are not required.
         * When they become unrequired they deactivate themselves.
         *
         * TODO Future problem: Matter that allows for multiple sources
         * How to handle a situation when a binding scrall that uses a certain source is already
         * actively using one but an additional source appears.
         * 
         */


        /*
         * _activeConjurings dictionary contains    ConjuringScrolls and Active LoomingScrolls
         * _activeBindings dictionary contains      Active AlteringScrolls and Active LoomingScrolls
         * _blockedBindings dictionary contains      Blocked AlteringScrolls and Blocked LoomingScrolls
         *
         * This means <<an active>> Looming Scroll is at every time present doubly in two dictionaries.
         *
         * TODO Find a cleaner solution
         */
        readonly Dictionary<Type, List<IConjuringScroll>> _activeConjurings = new();
        readonly Dictionary<Type, List<TBindingScroll>> _activeBindings = new();
        readonly Dictionary<Type, List<TBindingScroll>> _blockedBindings = new();

        Eris Eris { get; }

        public TheLibrary(Eris eris)
        {
            Eris = eris;
        }

        public bool IsConjurable<T>()
        {
            return _activeConjurings.ContainsKey(typeof(T));
        }

        #region Casting

        public void CastConjuring(IConjuringScroll scroll, bool wasJustCreated = false)
        {
            if (scroll.IsCastable is false) throw new ArgumentException("scroll is not castable");

            Eris.ScrollWillBeCast(scroll, isNew: wasJustCreated);

            // TODO add try catch
            scroll.Cast();

            SaveActiveConjuring(scroll);
        }

        public void CastLooming(ILoomingScroll scroll, bool wasJustCreated = false)
        {
            if (scroll.IsCastable is false) throw new ArgumentException("scroll is not castable");

            Eris.ScrollWillBeCast(scroll, isNew: wasJustCreated);

            scroll.Cast();

            SaveActiveConjuring(scroll);
            SaveActiveBinding(scroll);
        }

        public void CastWeaving(IAlteringScroll scroll, bool wasJustCreated = false)
        {
            if (scroll.IsCastable is false) throw new ArgumentException("scroll is not castable");

            Eris.ScrollWillBeCast(scroll, isNew: wasJustCreated);

            // TODO make sure reverse folding exist

            // TODO add try catch
            scroll.Cast();

            SaveActiveBinding(scroll);
        }

        #endregion

        #region Saving

        void SaveActiveConjuring(IConjuringScroll scroll)
        {
            // TODO make sure reverse folding exist

            Type conjuredType = scroll.ConjuredType;

            if (_activeConjurings.ContainsKey(conjuredType) is false)
            {
                _activeConjurings[conjuredType] = new List<IConjuringScroll>();
            }

            // TODO UMMMM HANDLE MULTIPLE PROVIDERS OF SAME CONJURIJNG
            _activeConjurings[conjuredType].Add(scroll);

            UnblockScrolls(conjuredType);
        }

        void SaveActiveBinding(TBindingScroll scroll)
        {
            foreach (Type req in scroll.Requirements)
            {
                if (scroll[req])
                {
                    if (_activeBindings.ContainsKey(req) is false)
                    {
                        _activeBindings[req] = new List<TBindingScroll>();
                    }

                    _activeBindings[req].Add(scroll);
                }
                else
                    throw new Exception(
                        "This scroll shouldn't be saved as an active binding since it has an inactive dependency.");
            }
        }

        public void SaveBlockedBinding(TBindingScroll scroll, bool wasJustCreated = false)
        {
            Eris.ScrollWillBeBlocked(scroll, isNew: wasJustCreated);

            foreach (Type requirement in scroll.Requirements)
            {
                if (scroll[requirement]) continue; // not blocked here, thats why indexers can suck

                if (_blockedBindings.ContainsKey(requirement) is false)
                {
                    _blockedBindings[requirement] = new List<TBindingScroll>();
                }

                _blockedBindings[requirement].Add(scroll);
            }
        }

        #endregion

        #region (Un)blocking

        public void CheckBindingScrollRequirements(TBindingScroll bindingScroll)
        {
            foreach (var req in bindingScroll.Requirements)
            {
                bindingScroll[req] = _activeConjurings.ContainsKey(req);
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
                    case ILoomingScroll loomingScroll:
                        CastLooming(loomingScroll); // ! recurse inside
                        break;
                    case IAlteringScroll alteringScroll:
                        CastWeaving(alteringScroll);
                        break;
                    default:
                        throw new Exception("Unhandled new scroll type.");
                }

                RemoveBlockedBinding(unblockedType, unblockedScroll);
            }
        }

        #endregion

        #region Removing

        void RemoveAllBlockedBindings(TBindingScroll unblockedScroll)
        {
            foreach (var kvp in unblockedScroll.AvailableIngredientsDictionary)
            {
                if (kvp.Value == false)
                {
                    RemoveBlockedBinding(kvp.Key, unblockedScroll);
                }
            }
        }

        void RemoveAllActiveBindings(TBindingScroll scroll)
        {
            foreach (var kvp in scroll.AvailableIngredientsDictionary)
            {
                bool isAvailable = kvp.Value;
                if (isAvailable)
                {
                    RemoveActiveBinding(kvp.Key, scroll);
                }
            }
        }

        // TODO LOOK AT USAGE dIN ALTERING SCROLL, DISPOSING ISN'T PROPERLY HANDLED YET
        public void RemoveBlockedBinding(Type blockingType, TBindingScroll unblockedScroll)
        {
            // ! this can be 0, it is slightly inefficient if the scroll was unblocked by a given type before
            // todo guid check
            int removeCount = _blockedBindings[blockingType]
                .RemoveAll(scroll => scroll.Guid == unblockedScroll.Guid);

            if (removeCount > 1)
                throw new Exception(
                    "Something weird happened. A single scroll shouldn't have been saved more than once for a blocked binding by conjuring type.");

            if (_blockedBindings[blockingType].Count == 0)
            {
                _blockedBindings.Remove(blockingType);
            }
        }

        public void RemoveActiveBinding(Type boundType, TBindingScroll boundScroll)
        {
            int removeCount = _activeBindings[boundType]
                .RemoveAll(scroll => scroll.Guid == boundScroll.Guid);

            if (removeCount > 1)
                throw new Exception(
                    "Something weird happened. A single scroll shouldn't have been saved more than once as an active binding for a conjuring type.");

            if (_activeBindings[boundType].Count == 0)
            {
                _activeBindings.Remove(boundType);
            }
        }

        public void RemoveActiveConjuring(IConjuringScroll scroll)
        {
            Type removedConjuring = scroll.ConjuredType;

            if (_activeConjurings[removedConjuring].RemoveAll(x => x.Guid == scroll.Guid) == 0)
                throw new Exception("Conjuring Scroll was not an active conjuring.");

            if (_activeConjurings[removedConjuring].Count == 0)
            {
                _activeConjurings.Remove(removedConjuring);
            }

            CheckForActiveBindingsDependingOnLostConjuring(removedConjuring);
        }

        void CheckForActiveBindingsDependingOnLostConjuring(Type removedConjuring)
        {
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
                    SaveBlockedBinding(newlyBlocked);

                    if (newlyBlocked is ILoomingScroll loomingScroll)
                    {
                        RemoveActiveConjuring(loomingScroll); // ! recurse
                    }
                }
            }
        }

        #endregion

        #region Forgetting Scrolls

        public void ForgetLoomScroll<Q>(TScrollBase scroll)
            where Q : TMatter
        {
            Eris.ScrollWillBeForgotten(scroll, isNew: false);

            Type removedSpellType = scroll.GetType();

            if (scroll is ILoomingScroll<Q> looming)
            {
                // TODO THIS IS WRONG, SHOULD BE MORE LIKE WASCAST HERE
                if (looming.IsCastable)
                {
                    RemoveActiveConjuring(looming);

                    // TODO this is an unclear spelling, it means to remove all catalogues active
                    // TODO binding for this scroll
                    RemoveAllActiveBindings(looming);
                }
                else RemoveAllBlockedBindings(looming);
            }
            else
            {
                throw new NotImplementedException($"Typeof: {removedSpellType}");
            }
        }

        public void ForgetConjuringScroll(IConjuringScroll scroll)
        {
            Eris.ScrollWillBeForgotten(scroll, isNew: false);
            RemoveActiveConjuring(scroll);
        }

        public void ForgetWeavingScroll(IAlteringScroll scroll)
        {
            Eris.ScrollWillBeForgotten(scroll, isNew: false);
            if (scroll.WasCast) RemoveAllActiveBindings(scroll);
            else RemoveAllBlockedBindings(scroll);
        }

        #endregion

        public IObservable<T> AskForIngredient<T>() where T : TMatter
        {
            Type type = typeof(T);

            if (!_activeConjurings.ContainsKey(type))
                throw new Exception("it shouldnt be possible since we checcked for iscastable");

            List<IConjuringScroll> scrolls = _activeConjurings[type];

            // TODO HANDLING MULTIPLE PROVIDERS OF A SAME MATTER TYPE
            if (scrolls.Count > 1) throw new NotImplementedException("multiple castable scrolls of same type");
            var conjuringScroll = scrolls[0] as TConjuringScroll<T>;

            if (conjuringScroll == null)
                throw new Exception("Failed to get an ingredient while it was registered as available.");
            
            return conjuringScroll.GetConjuring();
        }
    }
}