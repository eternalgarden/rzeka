using System;
using System.Collections.Generic;

namespace Rzeka
{
    public struct SerializableWeaving : ISerlializableBindingSpell
    {
        public Guid Guid { get; set; }
        public string Title { get; set; }
        public SpellSchool SpellSchool { get; set; }
        public string WhosName { get; set; }
        public bool WasCast { get; set; }
        public bool HasMana { get; set; }
        public Dictionary<string, SerializableStranding[]> Ingredients { get; set; }
    }

    public interface TAlteringScroll : TBindingScroll
    {

    }
}