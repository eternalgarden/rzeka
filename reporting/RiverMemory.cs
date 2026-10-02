using System.Runtime.CompilerServices;

namespace Rzeka.Reporting;

internal readonly record struct Provenance(DateTimeOffset At, SpellRef ShapedBy);

// What the reporter remembers about recent river activity.
// Holds only strings and IDs, never matter objects (or their payloads), 
// so it keeps nothing alive. Works exclusively on River thread.
internal sealed class RiverMemory(int provenanceCapacity = 1000, int breadcrumbCapacity = 20)
{
    readonly Dictionary<Guid, Provenance> _provByMatterId = [];
    readonly Queue<Guid> _provOrder = new();
    readonly Queue<Breadcrumb> _breadcrumbs = new();
    readonly ConditionalWeakTable<ISpell, SpellRef> _spellRefs = [];

    public void RecordShaped(IMatter matter, ISpell spell, DateTimeOffset at)
    {
        SpellRef shapedBy = _spellRefs.GetValue(spell, Describe);
        DateTimeOffset utc = at.ToUniversalTime();

        // The first shaping is the matter's origin; later occurrences of the same Guid
        // (e.g. a clone re-stamped by a Loom) keep it.
        if (_provByMatterId.TryAdd(matter.Guid, new Provenance(utc, shapedBy)))
        {
            _provOrder.Enqueue(matter.Guid);
            if (_provOrder.Count > provenanceCapacity)
                _provByMatterId.Remove(_provOrder.Dequeue());
        }

        _breadcrumbs.Enqueue(new Breadcrumb(utc, matter.GetType().Name, shapedBy.Title));
        if (_breadcrumbs.Count > breadcrumbCapacity)
            _breadcrumbs.Dequeue();
    }

    public Provenance? ProvenanceOf(Guid matterId) =>
        _provByMatterId.TryGetValue(matterId, out Provenance provenance) ? provenance : null;

    public IReadOnlyList<Breadcrumb> Breadcrumbs() => [.. _breadcrumbs];

    // Title, SpellSchool and Who's name
    public static SpellRef Describe(ISpell spell) =>
        new(spell.Title, spell.SpellSchool.ToString(), spell.Who.GetType().Name);
}
