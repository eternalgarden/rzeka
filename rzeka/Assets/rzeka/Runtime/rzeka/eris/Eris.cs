using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine;

namespace Rzeka
{
    public interface IErisEmanationCapable
    {
        void ReceiveSpellOccurence(SerializableSpellOccurence occurence);
        void ReceiveMatterOccurence(SerializableMatterOccurence occurence);
    }

    public class Eris : IDisposable
    {
        public IErisEmanationCapable Emanation { get; set; }
        public IObservable<SpellOccurence> SpellOccurences { get; }
        public IObservable<MatterOccurence> MatterOccurences { get; }

        CollectibleDisposable _disposables;

        [Obsolete] public readonly IObservable<RealmEvent> RealmEventStream;

        Queue<SerializableSpellOccurence> spellOccQueue = new();
        Queue<SerializableMatterOccurence> matterOccQueue = new();

        public Eris(IObservable<SpellOccurence> spellOccurences, IObservable<MatterOccurence> matterOccurences)
        {
            _disposables = new CollectibleDisposable();

            SpellOccurences = spellOccurences;
            MatterOccurences = matterOccurences;

            _disposables += SpellOccurences.Subscribe(occ => {

                var serializableOcc = new SerializableSpellOccurence() {
                    Guid = occ.Guid,
                    Timestamp = occ.Timestamp,
                    Source = GetSerializableSpell(occ.Source),
                    SpellOccurenceCategory = occ.SpellOccurenceCategory
                };

                if (Emanation is null)
                {
                    spellOccQueue.Enqueue(serializableOcc);
                    return;
                }
                else if (spellOccQueue.Count > 0)
                {
                    while (spellOccQueue.Count > 0)
                    {
                        Emanation.ReceiveSpellOccurence(spellOccQueue.Dequeue());
                    }
                }

                Emanation.ReceiveSpellOccurence(serializableOcc);
            });

            _disposables += MatterOccurences.Subscribe(occ => {

                var serializableOcc = new SerializableMatterOccurence() {
                    Guid = occ.Guid,
                    Timestamp = occ.Timestamp,
                    Source = GetSerializableSpell(occ.Source),
                    Matter = occ.Matter,
                    MatterTypeName = occ.Matter.GetType().Name,
                    MatterOccurenceCategory = occ.MatterOccurenceCategory
                };

                if (Emanation is null)
                {
                    matterOccQueue.Enqueue(serializableOcc);
                    return;
                }
                else if (matterOccQueue.Count > 0)
                {
                    while (matterOccQueue.Count > 0)
                    {
                        Emanation.ReceiveMatterOccurence(matterOccQueue.Dequeue());
                    }
                }

                Emanation.ReceiveMatterOccurence(serializableOcc);
            });
        }

        // ? move this to the scroll so it wont have to be created each time if that makes sense
        ISerializableSpell GetSerializableSpell(TScrollBase source)
        {
            ISerializableSpell spell;

            if (source.SpellSchool is SpellSchool.Looming)
            {
                spell = GetSerializableLooming(source);
            }
            else if (source.SpellSchool is SpellSchool.Stranding)
            {
                spell = GetSerializableStranding(source);
            }
            else
            {
                spell = GetSerializableWeaving(source);
            }

            spell.Guid = source.Guid;
            spell.Title = source.Title;
            spell.WhosName = source.Who is MonoBehaviour 
                ? $"{(source.Who as MonoBehaviour).gameObject.name}'s {source.Who.GetType().Name}"
                : source.Who.GetType().Name;
            spell.WasCast = source.WasCast;

            return spell;
        }

        private SerializableStranding GetSerializableStranding(TScrollBase source)
        {
            IConjuringScroll conjuring = source as IConjuringScroll;

            SerializableStranding stranding = new SerializableStranding() {
                SpellSchool = SpellSchool.Stranding,
                ConjuredType = conjuring.ConjuredType.Name
            };

            return stranding;
        }

        private SerializableLooming GetSerializableLooming(TScrollBase source)
        {
            TBindingScroll binding = source as TBindingScroll;
            IConjuringScroll conjuring = source as IConjuringScroll;

            SerializableLooming looming = new SerializableLooming()
            {
                SpellSchool = SpellSchool.Looming,
                Ingredients = GetSerializableIngredients(binding),
                WasCast = binding.WasCast,
                ConjuredType = conjuring.ConjuredType.Name,
                HasMana = binding.HasMana
            };

            return looming;
        }

        private SerializableWeaving GetSerializableWeaving(TScrollBase source)
        {
            TBindingScroll binding = source as TBindingScroll;

            SerializableWeaving weaving = new SerializableWeaving()
            {
                SpellSchool = SpellSchool.Weaving,
                Ingredients = GetSerializableIngredients(binding),
                WasCast = binding.WasCast,
                HasMana = binding.HasMana
            };

            return weaving;
        }

        private Dictionary<string, SerializableStranding[]> GetSerializableIngredients(TBindingScroll binding)
        {
            return binding
                .Ingredients
                .Select(kvp => new KeyValuePair<string, SerializableStranding[]>(
                    key: kvp.Key.Name,
                    value: kvp.Value.Select(conjuring => GetSerializableStranding(conjuring)).ToArray())
                )
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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