using System;
using System.Collections.Generic;

namespace Rzeka
{

    [Serializable]
    public struct SerializableLooming : ISerializableConjuringSpell, ISerlializableBindingSpell
    {
        public Guid Guid { get; set; }
        public string Title { get; set; }
        public SpellType SpellType { get; set; }
        public string Who { get; set; }
        public bool WasCast { get; set; }

        public string ConjuredType { get; set; }
        public bool HasMana { get; set; }
        public Dictionary<string, SerializableStranding[]> Ingredients { get; set; }
    }

    public interface TLoomingScroll<Q> : TBindingScroll, TConjuringScroll<Q> 
        where Q : TMatter
    {

    }
}