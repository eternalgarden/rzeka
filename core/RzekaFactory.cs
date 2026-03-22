using System.Collections.Generic;
using System.Linq;

namespace Rzeka
{
    public class RzekaFactory
    {
        readonly List<SpringRiver> _instances = new();

        public SpringRiver Create(string name, RzekaRole role = RzekaRole.Local)
        {
            var rzeka = new SpringRiver(name, role);
            _instances.Add(rzeka);
            return rzeka;
        }

        public IEnumerable<Eris> AllErises => _instances.Select(r => r.Eris);
    }
}
