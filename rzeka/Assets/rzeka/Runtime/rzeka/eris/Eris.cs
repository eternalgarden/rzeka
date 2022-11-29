using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine;

namespace Rzeka
{
    public interface IEmanationCapable
    {
        void ReceiveSpellOccurence(SerializableSpellOccurence occurence);
        void ReceiveMatterOccurence(SerializableMatterOccurence occurence);
    }

    public class Eris : IDisposable
    {
        public IEmanationCapable Emanation { get; set; }
        public IObservable<SpellOccurence> SpellOccurences { get; }
        public IObservable<MatterOccurence> MatterOccurences { get; }

        CollectibleDisposable _disposables;

        [Obsolete] public readonly IObservable<RealmEvent> RealmEventStream;

        public Eris(IObservable<SpellOccurence> spellOccurences, IObservable<MatterOccurence> matterOccurences)
        {
            _disposables = new CollectibleDisposable();

            SpellOccurences = spellOccurences;
            MatterOccurences = matterOccurences;

            _disposables += SpellOccurences.Subscribe(occ => {
                Debug.Log($"<color=yellow>{occ.SpellType} {occ.SpellOccurenceCategory}</color>");
                
                // Emanation.ReceiveSpellOccurence(new SerializableSpellOccurence() {
                //     Guid = occ.Guid,
                //     Timestamp = occ.Timestamp,
                //     Spell = GetSerializableSpell(occ.Scroll),
                //     SpellType = occ.SpellType,
                //     SpellOccurenceCategory = occ.SpellOccurenceCategory
                // });
            });

            _disposables += MatterOccurences.Subscribe(occ => {
                Debug.Log($"<color=cyan>{occ.Matter.GetType().Name} {occ.MatterOccurenceCategory}</color>");

                // Emanation.ReceiveMatterOccurence(new SerializableMatterOccurence() {
                //     Guid = occ.Guid,
                //     Timestamp = occ.Timestamp,
                //     Spell = GetSerializableSpell(occ.Scroll),
                //     Matter = occ.Matter,
                //     MatterOccurenceCategory = occ.MatterOccurenceCategory
                // });
            });
        }

        // TODO move this to the scroll so it wont have to be created each time
        ISerializableSpell GetSerializableSpell(TScrollBase source)
        {
            return null;
        }
        
        public void Dispose()
        {
            _disposables.Dispose();
        }

        void Print(string color, string head, string msg, params object[] args)
        {
            string text = string.Format(msg, args);
        }
    }
}