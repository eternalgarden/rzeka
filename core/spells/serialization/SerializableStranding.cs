using System;

namespace Rzeka;
  [Serializable]
public struct SerializableStranding : ISerializableStrandingSpell
{
    public Type conjuredType { get; set; }
    public Guid guid { get; set; }
    public string title { get; set; }
    public SpellSchool spellSchool { get; set; }
    public string whosName { get; set; }
    public bool hasMana { get; set; }
    public Who Who { get; set; }
}
