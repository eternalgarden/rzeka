
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Rzeka
{
    public class TheLibrary
    {
        Dictionary<Type, List<IConjuringScroll>> _castableConjuringScrolls = new(); // ! to contain IGivingSpell<T><T>
        Dictionary<Type, List<TBindingScroll>> _blockedScrollsByRequiredType = new();

        public void AddACastableScroll<T>(TConjuringScroll<T> scroll) where T : TMatter
        {
            AddACastableScroll(typeof(T), scroll);
        }

        public void AddACastableScroll(Type castableScrollType, IConjuringScroll scroll)
        {
            // ! $ LIBRARY.NEW_CASTABLE_CONJURING<Q>
            if (_castableConjuringScrolls.ContainsKey(castableScrollType) is false)
            {
                _castableConjuringScrolls[castableScrollType] = new List<IConjuringScroll>();
            }
            
            _castableConjuringScrolls[castableScrollType].Add(scroll);

            // todo check potential waiters in _spellsWaitingForIngredientOfType

            if (_blockedScrollsByRequiredType.ContainsKey(castableScrollType))
            {
                foreach (TBindingScroll blockedScroll in _blockedScrollsByRequiredType[castableScrollType])
                {
                    blockedScroll[castableScrollType] = true;

                    if (blockedScroll.IsCastable)
                    {
                        if (blockedScroll is IConjuringScroll conjuringScroll)
                        {
                            // ! recurse
                            AddACastableScroll(conjuringScroll.ConjuredType, conjuringScroll);
                            // TODO remove scroll from blocked ones but not during the foreach loop
                        }
                        else if (blockedScroll is TAlteringScroll alteringScroll)
                        {
                            // ! just cast it
                            alteringScroll.Cast(this);
                        }
                    }
                }
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

        public void RemoveABlockedScroll(TBindingScroll scroll)
        {
            foreach (Type req in scroll.Requirements)
            {
                if (scroll[req] == false)
                {
                    _blockedScrollsByRequiredType[req]
                        .First();

                    if (_blockedScrollsByRequiredType[req].Count == 0)
                    {
                        _blockedScrollsByRequiredType.Remove(req);
                    }
                }
            }
        }

        public void RemoveAKnownScroll<T>(IScrollBase scroll)
        {
            Type removedSpellType = typeof(T);

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
                    (bindingScroll)[req] = true;
                }
            }
        }
    }
}