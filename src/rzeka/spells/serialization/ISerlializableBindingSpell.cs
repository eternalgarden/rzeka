using System.Collections.Generic;

namespace Rzeka
{
  public interface ISerlializableBindingSpell : ISerializableSpell
    {
        // TODO this could be optimized to pass just a guid and grab specific details in js, but it's not necessary now
        Dictionary<string, bool> ingredients { get; set; }
    }
}