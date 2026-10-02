namespace Rzeka;

// A spell that failed: which spell, what was thrown, and the matter it was handling at the time
// (empty when there is none, e.g. a Strand whose source errored).
internal readonly record struct Miscast(ISpell Spell, Exception Exception, IReadOnlyList<IMatter> Trigger);
