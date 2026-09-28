namespace Rzeka.Reporting;

internal static class CausalChain
{
    public const int MaxNodes = 50;

    // Breadth-first from the triggers, so the closest causes survive the cap. Circumstances form
    // a graph (a Loom2 output has two parents, ancestors are shared), hence the visited set.
    public static ChainInfo Walk(
        IReadOnlyList<IMatter> triggers,
        Func<Guid, Provenance?> provenanceOf,
        int maxNodes = MaxNodes
    )
    {
        var nodes = new List<ChainNode>();
        var visited = new HashSet<Guid>();
        var pending = new Queue<IMatter>();
        bool truncated = false;

        foreach (IMatter trigger in triggers)
            if (visited.Add(trigger.Guid))
                pending.Enqueue(trigger);

        while (pending.Count > 0)
        {
            if (nodes.Count == maxNodes)
            {
                truncated = true;
                break;
            }

            IMatter matter = pending.Dequeue();
            IMatter[] causes = matter.Circumstances.Where(c => c is not null).ToArray();
            foreach (IMatter cause in causes)
                if (visited.Add(cause.Guid))
                    pending.Enqueue(cause);

            Provenance? provenance = provenanceOf(matter.Guid);
            nodes.Add(
                new ChainNode(
                    matter.Guid,
                    matter.GetType().Name,
                    provenance?.At,
                    provenance?.ShapedBy,
                    causes.Select(c => c.Guid).ToArray()
                )
            );
        }

        return new ChainInfo(nodes, truncated);
    }
}
