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
        Dictionary<Type, List<IConjuringScroll>> _castableConjuringScrolls = new(); // ! to contain IGivingSpell<T><T>
        Dictionary<Type, List<TBindingScroll>> _blockedScrollsByRequiredType = new();

        Eris Eris { get; }

        public TheLibrary(Eris eris)
        {
            Eris = eris;
        }

        public bool IsConjurable<T>(out IConjuringScroll[] conjurers)
        {
            if (_castableConjuringScrolls.ContainsKey(typeof(T)) is false)
            {
                conjurers = null;
                return false;
            }

            conjurers = _castableConjuringScrolls[typeof(T)].ToArray();
            return true;
        }

        public bool IsTypeBlockingSpells<T>(out TBindingScroll[] blockedScrolls)
        {
            if (_blockedScrollsByRequiredType.ContainsKey(typeof(T)))
            {
                blockedScrolls = _blockedScrollsByRequiredType[typeof(T)].ToArray();
                return true;
            }
            else
            {
                blockedScrolls = null;
                return false;
            }
        }

        public void AddConjuringScroll<T>(TConjuringScroll<T> scroll) where T : TMatter
        {
            AddConjuringScroll(typeof(T), scroll);
        }

        public void AddConjuringScroll(Type castableScrollType, IConjuringScroll scroll)
        {
            if (scroll.IsCastable is false) throw new ArgumentException("scroll is not castable");

            // ! $ LIBRARY.NEW_CASTABLE_CONJURING<Q>
            if (_castableConjuringScrolls.ContainsKey(castableScrollType) is false)
            {
                _castableConjuringScrolls[castableScrollType] = new List<IConjuringScroll>();
            }

            _castableConjuringScrolls[castableScrollType].Add(scroll);

            CheckIfNewConjuringScrollCouldBeOfUse(castableScrollType);
        }

        void CheckIfNewConjuringScrollCouldBeOfUse(Type newConjuringScrollType)
        {
            if (_blockedScrollsByRequiredType.ContainsKey(newConjuringScrollType))
            {
                List<TBindingScroll> unblockedScrolls = new();
                foreach (TBindingScroll blockedScroll in _blockedScrollsByRequiredType[newConjuringScrollType])
                {
                    blockedScroll[newConjuringScrollType] = true;

                    // todo at one time it might be better to differentiate between altering scrolls here who overload iscastable
                    if (blockedScroll.IsCastable) unblockedScrolls.Add(blockedScroll);
                }

                UnblockScrolls(newConjuringScrollType, unblockedScrolls.ToArray());
            }
        }

        void UnblockScrolls(Type unblockedType, TBindingScroll[] scrolls)
        {
            foreach (TBindingScroll unblockedScroll in scrolls)
            {
                switch (unblockedScroll)
                {
                    case IConjuringScroll conjuringScroll: // * These are the looming scrolls
                        // ! recurse
                        AddConjuringScroll(conjuringScroll.ConjuredType, conjuringScroll);
                        break;
                    // ! just cast it
                    case TAlteringScroll { IsCastable: true } alteringScroll:
                        Eris.ScrollWillBeCast(alteringScroll, isNew: false);
                        // todo this will be good to actually hold all scrolls reference to make sure there are no undisposed things
                        alteringScroll.Cast();
                        break;
                    default:
                        throw new Exception("smth rlly wrong");
                }

                RemoveFromBlockedScrollsCollection(unblockedType, unblockedScroll);
            }
        }

        public void AddABlockedScroll(TBindingScroll scroll)
        {
            foreach (Type req in scroll.Requirements)
            {
                if (scroll[req] == false)
                {
                    if (_blockedScrollsByRequiredType.ContainsKey(req) is false)
                    {
                        _blockedScrollsByRequiredType[req] = new List<TBindingScroll>();
                    }

                    _blockedScrollsByRequiredType[req].Add(scroll);
                }
            }
        }

        public void RemoveFromAllBlockedScrollsCollections(TBindingScroll unblockedScroll)
        {
            foreach (var kvp in unblockedScroll.AvailableIngredientsDictionary)
            {
                if (kvp.Value == false)
                {
                    RemoveFromBlockedScrollsCollection(kvp.Key, unblockedScroll);
                }
            }
        }

        public void RemoveFromBlockedScrollsCollection(Type blockingType, TBindingScroll unblockedScroll)
        {
            // ! this can be 0, it is slightly inefficient if the scroll was unblocked by a given type before
            // todo guid check
            int removeCount = _blockedScrollsByRequiredType[blockingType]
                .RemoveAll(scroll => scroll.Guid == unblockedScroll.Guid);

            if (removeCount > 1) throw new Exception("something weird happened");

            if (_blockedScrollsByRequiredType[blockingType].Count == 0)
            {
                _blockedScrollsByRequiredType.Remove(blockingType);
            }
        }

        // todo rename this thing it is really misleading and doesnt seem to handle AlteringScrolls
        public void ForgetLoomScroll<Q>(TScrollBase scroll)
            where Q : TMatter
        {
            // ! $ LIBRARY.FORGETTING_A_SCROLL<Q>

            Type removedSpellType = scroll.GetType();

            if (scroll is ILoomingScroll<Q> looming)
            {
                if (looming.IsCastable) RemoveFromConjuringScrolls(looming);
                else RemoveFromAllBlockedScrollsCollections(looming);
            }
            else
            {
                throw new NotImplementedException($"Typeof: {removedSpellType}");
            }
        }

        public void RemoveFromConjuringScrolls(IConjuringScroll scroll)
        {
            Type removedSpellType = scroll.ConjuredType;

            //if (_castableConjuringScrolls[removedSpellType].Contains(scroll) is false) throw new Exception("this is bad");
            if (_castableConjuringScrolls[removedSpellType].RemoveAll(x => x.Guid == scroll.Guid) == 0)
                throw new Exception("unexpected bewware");

            if (_castableConjuringScrolls[removedSpellType].Count == 0)
            {
                //Debug.Log($"removing type {removedSpellType}");
                _castableConjuringScrolls.Remove(removedSpellType);
            }

            List<(Type key, TBindingScroll scroll)> newBlockedScrolls = new();

            var thing = _castableConjuringScrolls
                .ToObservable()
                .Where(kvp => kvp.Value is TBindingScroll)
                .Select(kvp => (key: kvp.Key, spell: kvp.Value as TBindingScroll))
                .Where(o => o.spell.Requirements.Contains(removedSpellType))
                .Subscribe(o =>
                {
                    //throw new Exception("Test");
                    o.spell[removedSpellType] = false;
                    newBlockedScrolls.Add((o.key, o.spell));
                });

            foreach (var newBlockedScroll in newBlockedScrolls)
            {
                // TODO now add a check for existing blocked scrolls
                AddABlockedScroll(newBlockedScroll.scroll);

                if (newBlockedScroll.scroll is IConjuringScroll conjuringScroll)
                {
                    RemoveFromConjuringScrolls(conjuringScroll); // ! recurse
                }
            }
        }

        public void AskForIngredient<T>(out IObservable<T> ingredient) where T : TMatter
        {
            Type type = typeof(T);

            if (!_castableConjuringScrolls.ContainsKey(type))
                throw new Exception("it shouldnt be possible since we checcked for iscastable");

            List<IConjuringScroll> scrolls = _castableConjuringScrolls[type];

            // TODO HANDLING MULTIPLE PROVIDERS OF A SAME MATTER TYPE
            if (scrolls.Count > 1) throw new NotImplementedException("multiple castable scrolls of same type");
            var conjuringScroll = scrolls[0] as TConjuringScroll<T>;

            Eris.ScrollWillBeCast(conjuringScroll, isNew: false);

            if (conjuringScroll.TryGetConjuring(out ingredient) is false)
            {
                throw new Exception("Failed to get a conjuring when one should become available");
            }
        }

        public void CheckBindingScrollRequirements(TBindingScroll bindingScroll)
        {
            foreach (var req in bindingScroll.Requirements)
            {
                if (_castableConjuringScrolls.ContainsKey(req))
                {
                    //Debug.Log("yap");

                    (bindingScroll)[req] = true;
                }
            }
        }
    }
}