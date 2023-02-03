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

    public enum OccurenceCategory
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
        public Exception Exception { get; }

        public ExceptionalLuggage(Exception exception)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }
    }

    public interface Occurence
    {
        Guid Guid { get; set; }
        DateTimeOffset Timestamp  { get; set; }
        TSpell Source { get; set; }
        Luggage Luggage { get; set; }
    }

    public struct SpellOccurence : Occurence
    {
        public Guid Guid { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TSpell Source { get; set; }
        public Luggage Luggage { get; set; }

        public SpellOccurenceCategory SpellOccurenceCategory { get; set; }
    }

    public struct MatterOccurence : Occurence
    {
        public Guid Guid { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TSpell Source { get; set; }
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
        public OccurenceCategory occurenceCategory => OccurenceCategory.Spell;
        public Guid guid { get; set; }
        public long timestamp  { get; set; } // in unix milliseconds
        public ISerializableSpell spell { get; set; }
        
        public SpellOccurenceCategory spellOccurenceCategory { get; set; }
    }

    public struct SerializableMatterOccurence
    {
        public OccurenceCategory occurenceCategory => OccurenceCategory.Matter;
        public Guid guid { get; set; }
        public long timestamp  { get; set; } // in unix seconds
        public ISerializableSpell spell { get; set; }

        public MatterOccurenceCategory matterOccurenceCategory { get; set; }
        public Type matterType { get; set; } // * we use a custom serializer for Type
        public TMatter matter { get; set; }
    }
    
    #endregion // ---------------------------------- Serializable Occurences -------------------------
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 06 November 2022 🌊 */