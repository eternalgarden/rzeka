/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/

namespace Rzeka
{
    public enum OccurenceCategory
    {
        Spell,
        Matter,
        Message,
    }

    public enum MatterOccurenceCategory
    {
        Shaped,
        Received,
    }

    public enum SpellOccurenceCategory
    {
        Created,
        HasMana,
        NoMana,
        // Wispd: the spell threw an exception during Cast() — signals an error state in the debugger.
        // Note: currently carries no exception details, only which spell errored.
        // To show the cause in the debugger, AlteringScroll.Cast() would need to attach the exception
        // to the occurence (see the commented-out code there).
        Wispd,
        Forgotten,
    }
}
