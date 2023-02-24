using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using UnityEngine;

namespace Rzeka
{
    public interface IErisEmanationCapable
    {
        void ReceiveSpellOccurence(SerializableSpellOccurence occurence);
        void ReceiveMatterOccurence(SerializableMatterOccurence occurence);
    }

    public interface IManaInformationProvideable
    {
        Type LastChangedType { get; }
        bool IsManaOfTypeAvailable<T>() where T : TMatter;
        bool IsManaOfTypeAvailable(Type type);
    }

    public class AvailableConjurers : IManaInformationProvideable
    {
        readonly Dictionary<Type, HashSet<Guid>> _availableConjurers = new();

        public Type LastChangedType { get; private set; }

        public void ActivateConjurer([NotNull] IConjuringSpell conjuringSpell)
        {
            if (conjuringSpell == null) throw new ArgumentNullException(nameof(conjuringSpell));
            
            Type key = conjuringSpell.ConjuredType;
            if (_availableConjurers.ContainsKey(key) is false)
            {
                _availableConjurers[key] = new HashSet<Guid>();
            } 
            
            _availableConjurers[key].Add(conjuringSpell.Guid);

            LastChangedType = key;
        }
        
        public void DectivateConjurer([NotNull] IConjuringSpell conjuringSpell)
        {
            if (conjuringSpell == null) throw new ArgumentNullException(nameof(conjuringSpell));
            
            Type key = conjuringSpell.ConjuredType;

            if (_availableConjurers.ContainsKey(key) is false) return;
            if (!_availableConjurers[key].Contains(conjuringSpell.Guid)) return;
            
            // TODO this could use some testing if things are properly removed
            _availableConjurers[key].Remove(conjuringSpell.Guid);
            
            LastChangedType = key;
        }

        public bool IsManaOfTypeAvailable<T>() where T : TMatter
        {
            return IsManaOfTypeAvailable(typeof(T));
        }
        
        public bool IsManaOfTypeAvailable(Type type)
        {
            return _availableConjurers.ContainsKey(type) && _availableConjurers[type].Count > 0;
        }
    }

    public class Eris : IDisposable
    {
        readonly Library _library;
        public IErisEmanationCapable Emanation { get; set; }

        CollectibleDisposable _disposables;

        Queue<SerializableSpellOccurence> spellOccQueue = new();
        Queue<SerializableMatterOccurence> matterOccQueue = new();

        public IObservable<SpellOccurence> SpellOccurences => SpellStream;
        public IObservable<MatterOccurence> MatterOccurences => MatterStream;

        Subject<SpellOccurence> SpellStream { get; }
        Subject<MatterOccurence> MatterStream { get; }
        
        IConnectableObservable<IManaInformationProvideable> _manaStream { get; }
        public IObservable<IManaInformationProvideable> ManaProvideableObservable => _manaStream;

        public void PublishSpellOccurence(SpellOccurence spellOccurence)
        {
            SpellStream.OnNext(spellOccurence);
        }
        
        public void PublishMatterOccurence(MatterOccurence matterOccurence)
        {
            MatterStream.OnNext(matterOccurence);
        }

        public Eris()
        {
            _disposables = new CollectibleDisposable();

            SpellStream = new Subject<SpellOccurence>();
            MatterStream = new Subject<MatterOccurence>();

            _manaStream = SpellStream
                .Where(occ => occ.Source.SpellSchool
                    is SpellSchool.Looming
                    or SpellSchool.Stranding)
                .Where(occ => occ.SpellOccurenceCategory
                    is SpellOccurenceCategory.HasMana
                    or SpellOccurenceCategory.NoMana
                    or SpellOccurenceCategory.Forgotten)
                .Scan((false, new AvailableConjurers()), (acc, current) =>
                {
                    AvailableConjurers accumulator = acc.Item2;

                    IConjuringSpell sourceAsConjuring =
                        current.Source as IConjuringSpell ?? throw new InvalidOperationException();

                    Type conjuredType = sourceAsConjuring.ConjuredType;
                    bool wasManaAvailable = accumulator.IsManaOfTypeAvailable(conjuredType);

                    SpellOccurenceCategory category = current.SpellOccurenceCategory;
                    if (category is SpellOccurenceCategory.HasMana)
                    {
                        accumulator.ActivateConjurer(sourceAsConjuring);
                    }
                    else
                    {
                        accumulator.DectivateConjurer(sourceAsConjuring);
                    }

                    bool isManaAvailable = accumulator.IsManaOfTypeAvailable(conjuredType);

                    bool hasAnythingChanged = wasManaAvailable != isManaAvailable;

                    return (hasAnythingChanged, accumulator);
                })
                .Where(accumulator => accumulator.Item1)
                .Select(accumulator => accumulator.Item2 as IManaInformationProvideable)
                .StartWith(new AvailableConjurers())
                .Multicast(new ReplaySubject<IManaInformationProvideable>(1));

            _disposables += _manaStream.Connect();

            _disposables += ManaProvideableObservable
                .Subscribe(info =>
                {
                    if (info.LastChangedType == null) return;
                    // Debug.Log($"next : {info.LastChangedType} {info.IsManaOfTypeAvailable(info.LastChangedType)}");
                });
            

            _disposables += SpellStream.Subscribe(occ => {

                var serializableOcc = new SerializableSpellOccurence() {
                    guid = occ.Guid,
                    timestamp = occ.Timestamp.ToUnixTimeSeconds(),
                    spell = GetSerializableSpell(occ.Source),
                    spellOccurenceCategory = occ.SpellOccurenceCategory
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

            _disposables += MatterStream.Subscribe(occ =>
            {

                var serializableOcc = new SerializableMatterOccurence() {
                    guid = occ.Guid,
                    timestamp = occ.Timestamp.ToUnixTimeSeconds(),
                    spell = GetSerializableSpell(occ.Source),
                    matter = occ.Matter,
                    matterType = occ.Matter.GetType(), // * custom serializer
                    matterOccurenceCategory = occ.MatterOccurenceCategory
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
        ISerializableSpell GetSerializableSpell(TSpell source)
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

            spell.guid = source.Guid;
            spell.title = source.Title;
            spell.whosName = source.Who is MonoBehaviour 
                ? $"{(source.Who as MonoBehaviour).gameObject.name}'s {source.Who.GetType().Name}"
                : source.Who.GetType().Name;
            spell.wasCast = source.HasMana;

            return spell;
        }

        private SerializableStranding GetSerializableStranding(TSpell source)
        {
            IConjuringSpell conjuring = source as IConjuringSpell;

            SerializableStranding stranding = new SerializableStranding() {
                spellSchool = SpellSchool.Stranding,
                conjuredType = conjuring.ConjuredType
            };

            return stranding;
        }

        private SerializableLooming GetSerializableLooming(TSpell source)
        {
            TBindingSpell binding = source as TBindingSpell;
            IConjuringSpell conjuring = source as IConjuringSpell;

            SerializableLooming looming = new SerializableLooming()
            {
                spellSchool = SpellSchool.Looming,
                ingredients = GetSerializableIngredients(binding),
                wasCast = binding.HasMana,
                conjuredType = conjuring.ConjuredType,
                hasMana = binding.HasMana // TODO this is a naming misguide
            };

            return looming;
        }

        private SerializableWeaving GetSerializableWeaving(TSpell source)
        {
            TBindingSpell binding = source as TBindingSpell;

            SerializableWeaving weaving = new SerializableWeaving()
            {
                spellSchool = SpellSchool.Weaving,
                ingredients = GetSerializableIngredients(binding),
                wasCast = binding.HasMana,
                hasMana = binding.HasMana // TODO this is a naming misguide
            };

            return weaving;
        }
        
        [Obsolete] // TODO there is a problem with that, there are no longer ingredients list
        private Dictionary<string, SerializableStranding[]> GetSerializableIngredients(TBindingSpell binding)
        {
            return binding
                .SatisfiedRequirements
                .Select(kvp => new KeyValuePair<string, SerializableStranding[]>(
                    key: kvp.Key.Name,
                    value: null) // oops
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