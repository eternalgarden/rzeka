using System;
using System.Collections.Generic;

namespace Rzeka
{
    public struct SerializableWeaving : ISerlializableBindingSpell
    {
        public string ConjuredType { get; set; }
        public Guid Guid { get; set; }
        public string Title { get; set; }
        public SpellType SpellType { get; set; }
        public string Who { get; set; }
        public bool WasCast { get; set; }
        public bool HasMana { get; set; }
        public Dictionary<string, SerializableStranding[]> Ingredients { get; set; }
    }

    public interface TAlteringScroll : TBindingScroll
    {

    }
}