using System;

namespace Rzeka;
  public interface ISerializableStrandingSpell : ISerializableSpell
{
    Type conjuredType { get; set; }
}
