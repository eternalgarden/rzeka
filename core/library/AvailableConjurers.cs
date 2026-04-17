using System;
using System.Collections.Generic;

namespace Rzeka;
public class AvailableConjurers : IManaInformationProvideable
{
    readonly Dictionary<Type, HashSet<Guid>> _availableConjurers = new();

    public Type LastChangedType { get; private set; }

    public void ActivateConjurer(TStrandingSpell strandingSpell)
    {
        /* 🦠🦴 */

        if (strandingSpell == null) throw new ArgumentNullException(nameof(strandingSpell));

        Type key = strandingSpell.ConjuredType;
        if (_availableConjurers.ContainsKey(key) is false)
        {
            _availableConjurers[key] = new HashSet<Guid>();
        }

        _availableConjurers[key].Add(strandingSpell.Guid);

        LastChangedType = key;

        /* 🦠🦴 */
    }

    public void DectivateConjurer(TStrandingSpell strandingSpell)
    {
        /* 🧩 */

        if (strandingSpell == null) throw new ArgumentNullException(nameof(strandingSpell));

        Type key = strandingSpell.ConjuredType;

        if (_availableConjurers.ContainsKey(key) is false) return;
        if (!_availableConjurers[key].Contains(strandingSpell.Guid)) return;

        // TODO this could use some testing if things are properly removed
        _availableConjurers[key].Remove(strandingSpell.Guid);

        LastChangedType = key;

        /* 🧩 */
    }

    public bool IsManaOfTypeAvailable<T>() where T : TMatter
    {
        /* ⚒️⚗️🛠️ */

        return IsManaOfTypeAvailable(typeof(T));

        /* ⚒️⚗️🛠️ */
    }

    public bool IsManaOfTypeAvailable(Type type)
    {
        // TODO rework to consider stateful matter as always available
        return _availableConjurers.ContainsKey(type) && _availableConjurers[type].Count > 0;
    }
}
