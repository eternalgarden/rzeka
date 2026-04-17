using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

namespace Rzeka;
public class Spring
{
    readonly List<SpringRiver> _instances = new();
    readonly Subject<SpringRiver> _instanceCreated = new();

    public IRzeka Create(string name, RzekaRole role = RzekaRole.Local)
    {
        var rzeka = new SpringRiver(name, role);
        _instances.Add(rzeka);
        _instanceCreated.OnNext(rzeka);
        return rzeka;
    }

    internal IEnumerable<Eris> AllErises => _instances.Select(r => r.Eris);
    internal IObservable<SpringRiver> OnInstanceCreated => _instanceCreated;
}
