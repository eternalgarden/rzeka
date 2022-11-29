/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/

using System;

namespace Rzeka
{
    public enum SpellSchool 
    { 
        Stranding, // todo big renaming work after this
        Looming, 
        Weaving 
    }

    public enum OccurenceType
    {
        Spell,
        Matter
    }

    public enum MatterOccurenceCategory 
    { 
        Shaped, 
        Received, 
        Error 
    }

    public enum SpellOccurenceCategory 
    { 
        Created, 
        Cast, 
        NoMana, 
        Morphed, 
        Wispd,
        Forgotten 
    }

    public abstract class Luggage { }

    public class ExceptionalLuggage : Luggage
    {
        public Exception Exception { get; set; }
    }

    public interface Occurence
    {
        Guid Guid { get; set; }
        DateTimeOffset Timestamp  { get; set; }
        TScrollBase Source { get; set; }
        Luggage Luggage { get; set; }
    }

    public struct SpellOccurence : Occurence
    {
        public Guid Guid { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TScrollBase Source { get; set; }
        public Luggage Luggage { get; set; }

        public SpellOccurenceCategory SpellOccurenceCategory { get; set; }
    }

    public struct MatterOccurence : Occurence
    {
        public Guid Guid { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TScrollBase Source { get; set; }
        public Luggage Luggage { get; set; }

        public MatterOccurenceCategory MatterOccurenceCategory { get; set; }
        public TMatter Matter { get; set; }
    }

    
    //
    // ⛺ ─── Serializable Occurences ───────────────────────────────────────────────────
    //
    #region Serializable Occurences
    
    public struct SerializableSpellOccurence
    {
        public OccurenceType OccurenceType => OccurenceType.Spell;
        public Guid Guid { get; set; }
        public DateTimeOffset Timestamp  { get; set; }
        public ISerializableSpell Source { get; set; }
        
        public SpellOccurenceCategory SpellOccurenceCategory { get; set; }
    }

    public struct SerializableMatterOccurence
    {
        public OccurenceType OccurenceType => OccurenceType.Matter;
        public Guid Guid { get; set; }
        public DateTimeOffset Timestamp  { get; set; }
        public ISerializableSpell Source { get; set; }

        public MatterOccurenceCategory MatterOccurenceCategory { get; set; }
        public string MatterTypeName { get; set; }
        public TMatter Matter { get; set; }
    }
    
    #endregion // ---------------------------------- Serializable Occurences -------------------------
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 06 November 2022 🌊 */