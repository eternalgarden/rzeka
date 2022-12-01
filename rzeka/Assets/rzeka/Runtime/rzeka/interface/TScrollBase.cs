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

    public interface TScrollBase : IDisposable
    {
        public const float POST_CREATION_MANA_CHECK_DELAY = .3f; // in seconds

        /// <summary>
        /// This is because unity implemented default interfaces in a non-flat way ugh.
        /// And otherwise any time you would like to refer to it's defined methods you would have to cast it.
        /// </summary>
        TScrollBase ThisAsBase { get; } 
        SpellSchool SpellSchool { get; }
        Guid Guid { get; }
        object Who { get; }
        bool WasCast { get; }
        string Title { get; }
        ISubject<SpellOccurence> SpellStream { get; }
        ISubject<MatterOccurence> MatterStream { get; }
        CollectibleDisposable CollectionDisposable { get; set; }

        void Cast();

        // TODO centralize occurence creation
        void SendSpellOccurence(SpellOccurenceCategory occurenceCategory)
        {
            /* ⭐ ---- ---- */
            
            var occurence = new SpellOccurence
            {
                Guid = Guid.NewGuid(),
                Timestamp = DateTimeOffset.UtcNow,
                SpellOccurenceCategory = occurenceCategory,
                Source = this
            };

            SpellStream.OnNext(occurence);
            
            /* ---- ---- 🌠 */
        }

        void SendMatterOccurence(TMatter matter, MatterOccurenceCategory occurenceCategory)
        {
            /* ⭐ ---- ---- */
            
            var occurence = new MatterOccurence
            {
                Guid = Guid.NewGuid(),
                Timestamp = DateTimeOffset.UtcNow,
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
                Timestamp = DateTimeOffset.UtcNow,
                MatterOccurenceCategory = MatterOccurenceCategory.Error,
                Source = this,
                Luggage = new ExceptionalLuggage(ex)
            };

            MatterStream.OnNext(occurence);
            
            /* ---- ---- 🌠 */
        }

        void InitializeSpellBase()
        {
            if (CollectionDisposable is not null) return; // * hehe, this means it's already initialized

            CollectionDisposable = new();
            SendSpellOccurence(SpellOccurenceCategory.Created);
        }
    }
}