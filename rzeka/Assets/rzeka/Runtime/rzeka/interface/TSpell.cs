using System;
using System.Reactive.Subjects;

namespace Rzeka
{
    public interface ISerializableSpell
    {
        Guid guid { get; set; }
        string title { set; get; }
        SpellSchool spellSchool { get; set; }
        string whosName { get; set; }
        bool wasCast { get; set; }
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
        bool IsChanneling { get; protected set; }
        string Title { get; }
        ISubject<SpellOccurence> SpellStream { get; }
        ISubject<MatterOccurence> MatterStream { get; }
        Library Library { get; }
        CollectibleDisposable CollectionDisposable { get; set; }
        
        IObservable<bool> HasMana { get; }

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

            SpellStream.OnNext(occurence);
            
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

            MatterStream.OnNext(occurence);
            
            /* ---- ---- 🌠 */
        }

        void SendMatterExceptionOccurence(Exception ex)
        {
            /* ⭐ ---- ---- */
            
            var occurence = new MatterOccurence
            {
                Guid = Guid.NewGuid(),
                Timestamp = DateTimeOffset.Now,
                MatterOccurenceCategory = MatterOccurenceCategory.Error,
                Source = this,
                Luggage = new ExceptionalLuggage(ex)
            };

            MatterStream.OnNext(occurence);
            
            /* ---- ---- 🌠 */
        }

        void InitializeSpellBase()
        {
            /* ⭐ ---- ---- */
            
            if (CollectionDisposable is not null) return; // * hehe, this means it's already initialized
            CollectionDisposable = new();
            SendSpellOccurence(SpellOccurenceCategory.Created);
            
            /* ---- ---- 🌠 */
        }
    }
}