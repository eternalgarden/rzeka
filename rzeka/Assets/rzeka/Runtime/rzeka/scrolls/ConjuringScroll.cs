using System;

namespace Rzeka
{
    [Serializable]
    public class ConjuringScroll<Q> : TConjuringScroll<Q> where Q : TMatter
    {
        readonly IObservable<Q> spell;
        readonly TheLibrary library;
        readonly Eris eris;
        readonly object who;
        readonly Guid _guid = Guid.NewGuid();
        IObservable<Q> _observableSpell;

        public ConjuringScroll(object who, IObservable<Q> spell, TheLibrary library, Eris debugger)
        {
            this.who = who;
            this.spell = spell;
            this.library = library;
            this.eris = debugger;
        }

        public Guid Guid => _guid;
        public string Title => $"Conjuring of {typeof(Q).Name}";
        public Type ConjuredType => typeof(Q);
        public bool IsCastable => true;
        public object Who => who;
        public bool IsConjured => _observableSpell is not null;

        public IObservable<Q> ConjuredSpell
        {
            get => _observableSpell;
            set
            {
                if (_observableSpell is not null) throw new Exception("Was already cast");

                _observableSpell = value;
            }
        }

        public bool WasCast => ConjuredSpell is not null;

        public void Cast()
        {
            if (IsConjured is true) throw new Exception("Was already cast 🦇");

            ConjuredSpell = spell;
            // .DistinctUntilChanged(keySelector: next => next.Guid)
            // .Do(eris.GetReleasesObserver<Q>(this));
        }

        public IObservable<Q> GetConjuring()
        {
            if (IsConjured is false)
            {
                Cast();
            }

            // Debug.Log($"<color=cyan>getting conjurigng {GetType()}</color>");

            return ConjuredSpell;
        }

        public void Dispose()
        {
        }
    }
}