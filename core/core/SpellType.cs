/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/

namespace Rzeka
{
    public enum SpellSchool 
    { 
        Stranding,
        Looming, 
        Weaving 
    }

    public enum OccurenceCategory
    {
        Spell,
        Matter,
        Message
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
        Morphed, 
        Wispd,
        Forgotten 
    }

    public interface IOccurence
    {
        Guid Guid { get; set; }
        DateTimeOffset Timestamp { get; set; }
    }
    
    // TODO get rid of this derp, using messages instead
    [Obsolete]
    [Serializable]
    public class ExceptionOccurence : IOccurence
    {
        public Guid Guid { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TSpell Source { get; set; }
        public Exception Exception { get; set; }
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 06 November 2022 🌊 */
