using System;
using System.Collections.Generic;

namespace Rzeka;
  [Serializable]
public struct SerializableLooming : ISerializableStrandingSpell, ISerlializableBindingSpell
{
    public Guid guid { get; set; }
    public string title { get; set; }
    public SpellSchool spellSchool { get; set; }
    public string whosName { get; set; }
    public bool hasMana { get; set; }
    public Who Who { get; set; }

    public Type conjuredType { get; set; }
    public Dictionary<string, bool> ingredients { get; set; }
}
