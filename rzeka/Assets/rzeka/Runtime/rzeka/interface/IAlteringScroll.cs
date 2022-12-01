using System;
using System.Collections.Generic;

namespace Rzeka
{
    public struct SerializableWeaving : ISerlializableBindingSpell
    {
        public Guid guid { get; set; }
        public string title { get; set; }
        public SpellSchool spellSchool { get; set; }
        public string whosName { get; set; }
        public bool wasCast { get; set; }
        public bool hasMana { get; set; }
        public Dictionary<string, SerializableStranding[]> ingredients { get; set; }
    }

    public interface TAlteringScroll : TBindingScroll
    {

    }
}