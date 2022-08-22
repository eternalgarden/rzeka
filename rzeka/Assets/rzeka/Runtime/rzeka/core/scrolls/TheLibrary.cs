
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

        public bool IsConjurable<T>(out IConjuringScroll[] conjurers)
        {
            if (_castableConjuringScrolls.ContainsKey(typeof(T)))
            {
                conjurers = _castableConjuringScrolls[typeof(T)].ToArray();
                return true;
            }
            else
            {
                conjurers = null;
                return false;
            }
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

        private void CheckIfNewConjuringScrollCouldBeOfUse(Type newConjuringScrollType)
        {
            if (_blockedScrollsByRequiredType.ContainsKey(newConjuringScrollType))
            {
                List<TBindingScroll> unblockedScrolls = new();
                foreach (TBindingScroll blockedScroll in _blockedScrollsByRequiredType[newConjuringScrollType])
                {
                    blockedScroll[newConjuringScrollType] = true;

                    if (blockedScroll.IsCastable) unblockedScrolls.Add(blockedScroll);
                }
                UnblockScrolls(unblockedScrolls.ToArray());
            }
        }

        private void UnblockScrolls(TBindingScroll[] scrolls)
        {
            foreach (TBindingScroll unblockedScroll in scrolls)
            {
                if (unblockedScroll is IConjuringScroll conjuringScroll)
                {
                    // ! recurse
                    AddConjuringScroll(conjuringScroll.ConjuredType, conjuringScroll);
                }
                else if (unblockedScroll is TAlteringScroll alteringScroll)
                {
                    // ! just cast it
                    alteringScroll.Cast(this);
                }

                RemoveFromBlockedScrollsCollection(unblockedScroll);
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

        public void RemoveFromBlockedScrollsCollection(TBindingScroll unblockedScroll)
        {
            foreach (Type req in unblockedScroll.Requirements)
            {
                if (unblockedScroll[req] == true && _blockedScrollsByRequiredType.ContainsKey(req))
                {
                    // ! this can be 0, it is slightly inefficient if the scroll was unblocked by a given type before
                    // todo guid check
                    int removeCount = _blockedScrollsByRequiredType[req]
                        .RemoveAll(scroll => scroll.Guid == unblockedScroll.Guid);

                    if (removeCount > 1) throw new Exception("something weird happened");

                    if (_blockedScrollsByRequiredType[req].Count == 0)
                    {
                        _blockedScrollsByRequiredType.Remove(req);
                    }
                }
            }
        }

        public void ForgetScroll<Q>(TScrollBase scroll)
        {
            // ! $ LIBRARY.FORGETTING_A_SCROLL<Q>

            Type removedSpellType = typeof(Q);

            if (scroll.IsCastable && scroll is IConjuringScroll conjuringScroll)
            {
                RemoveFromConjuringScrolls(removedSpellType, conjuringScroll);
            }
            else
            {
                if (scroll is TBindingScroll bindingScroll)
                {
                    foreach (Type req in bindingScroll.Requirements)
                    {
                        if (bindingScroll[req] == false)
                        {
                            if (_blockedScrollsByRequiredType[req].Contains(bindingScroll) is false) throw new Exception("ummmm");
                            _blockedScrollsByRequiredType[req].Remove(bindingScroll);
                        }
                    }
                }
            }
        }

        public void RemoveFromConjuringScrolls(Type removedSpellType, IConjuringScroll scroll)
        {
            //if (_castableConjuringScrolls[removedSpellType].Contains(scroll) is false) throw new Exception("this is bad");
            if (_castableConjuringScrolls[removedSpellType].RemoveAll(x => x.Guid == scroll.Guid) == 0) throw new Exception("unexpected bewware");

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
                    RemoveFromConjuringScrolls(newBlockedScroll.key, conjuringScroll); // ! recurse
                }
            }
        }

        public bool AskForIngredient<T>(out IObservable<T> ingredient) where T : TMatter
        {
            ingredient = null;
            Type type = typeof(T);

            if (_castableConjuringScrolls.ContainsKey(type))
            {
                List<IConjuringScroll> scrolls = _castableConjuringScrolls[type];

                // todo handling multiple providers
                if (scrolls.Count > 1) throw new NotImplementedException("multiple castable scrolls of same type");

                var conjuringScroll = scrolls[0] as TConjuringScroll<T>;

                if (conjuringScroll.TryCast(out IObservable<T> givingSpell, this))
                {
                    ingredient = givingSpell;
                    return true;
                }
                else return false;
                //foreach (var scroll in scrolls)
                //{

                //}
            }
            else return false;
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