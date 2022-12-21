using System.Collections.Generic;

namespace Rzeka
{
  public interface ISerlializableBindingSpell : ISerializableSpell
    {
        bool hasMana { get; set; }

        // TODO this could be optimized to pass just a guid and grab specific details in js, but it's not necessary now
        Dictionary<string, SerializableStranding[]> ingredients { get; set; }
    }
}