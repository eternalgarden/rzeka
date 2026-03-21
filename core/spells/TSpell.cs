using System;

namespace Rzeka
{
    
    [Serializable]
    public struct Who
    {
        public Type WhosType { get; set; }
        public string? ParentGameObjectName { get; set; }
    }
    
    
    // TODO rework as an abstract class and add its implementations / generators to within specific spells
    public interface ISerializableSpell
    {
        Guid guid { get; set; }
        string title { set; get; }
        SpellSchool spellSchool { get; set; }
        string whosName { get; set; } 
        bool hasMana { get; set; } // TODO REMOVE
        Who Who { get; set; }
        // TODO ADD HASMANA
        
        // TODO REPLACE INGREDIENTS IN BINDING SPELL
        
        
    }

    public interface TSpell : IDisposable // TODO Rename to TSpell
    {
        // TODO this is rather unsafe
        public const float POST_CREATION_MANA_CHECK_DELAY = .3f; // in seconds

        /// <summary>
        /// This is because unity implemented default interfaces in a non-flat way ugh.
        /// And otherwise any time you would like to refer to it's defined methods you would have to cast it.
        /// </summary>
        TSpell ThisAsBase { get; } 
        SpellSchool SpellSchool { get; }
        Guid Guid { get; }
        object Who { get; }
        bool HasMana { get; protected set; }
        string Title { get; }
        Library Library { get; }
        Eris Eris { get; }
        CollectibleDisposable Q { get; set; }

        void Cast();

        void SendSpellOccurence(SpellOccurenceCategory occurenceCategory)
        {
            /* ⭐ ---- ---- */
            
            var occurence = new SpellOccurence
            {
                Guid = Guid.NewGuid(),
                Timestamp = DateTimeOffset.Now,
                SpellOccurenceCategory = occurenceCategory,
                Source = this
            };

            Eris.PublishSpellOccurence(occurence);
            
            /* ---- ---- 🌠 */
        }
        
        // Reformatted to avoid the boxing of value type implementing TMatter
        // Since we use structs for that
        // https://stackoverflow.com/questions/3032750/structs-interfaces-and-boxing
        void SendMatterOccurence<T>(T matter, MatterOccurenceCategory occurenceCategory)
            where T : TMatter
        {
            /* ⭐ ---- ---- */
            
            // Here boxing is unavoidable
            // TODO or is it? try adding generic version again
            // or consider running a different end user version with no access to Eris
            // if she would get too needy
            var occurence = new MatterOccurence
            {
                Guid = Guid.NewGuid(),
                Timestamp = DateTimeOffset.Now,
                Matter = matter,
                MatterOccurenceCategory = occurenceCategory,
                Source = this
            };

            Eris.PublishMatterOccurence(occurence);
            
            /* ---- ---- 🌠 */
        }

        void SendMatterExceptionOccurence(Exception ex)
        {
            /* ⭐ ---- ---- */
            
            var occurence = new ExceptionOccurence()
            {
                Guid = Guid.NewGuid(),
                Timestamp = DateTimeOffset.Now,
                Source = this,
                Exception = ex
            };

            Eris.PublishExceptionOccurence(occurence);
            
            /* ---- ---- 🌠 */
        }

        void InitializeSpellBase()
        {
            /* ⭐ ---- ---- */
            
            if (Q is not null) return; // * hehe, this means it's already initialized
            Q = new();
            SendSpellOccurence(SpellOccurenceCategory.Created);
            
            /* ---- ---- 🌠 */
        }
    }
}