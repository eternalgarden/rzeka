using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using UnityEngine;

namespace Rzeka
{
    public interface IErisConsulate
    {
        void OpenConsole();
        void ReceiveSpellOccurence(SerializableSpellOccurence occurence);
        void ReceiveMatterOccurence(SerializableMatterOccurence occurence);
        void ReceiveMessage(SerializableMessageOccurence messageOccurence);
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

        public void ActivateConjurer([NotNull] TStrandingSpell strandingSpell)
        {
            if (strandingSpell == null) throw new ArgumentNullException(nameof(strandingSpell));

            Type key = strandingSpell.ConjuredType;
            if (_availableConjurers.ContainsKey(key) is false)
            {
                _availableConjurers[key] = new HashSet<Guid>();
            }

            _availableConjurers[key].Add(strandingSpell.Guid);

            LastChangedType = key;
        }

        public void DectivateConjurer([NotNull] TStrandingSpell strandingSpell)
        {
            if (strandingSpell == null) throw new ArgumentNullException(nameof(strandingSpell));

            Type key = strandingSpell.ConjuredType;

            if (_availableConjurers.ContainsKey(key) is false) return;
            if (!_availableConjurers[key].Contains(strandingSpell.Guid)) return;

            // TODO this could use some testing if things are properly removed
            _availableConjurers[key].Remove(strandingSpell.Guid);

            LastChangedType = key;
        }

        public bool IsManaOfTypeAvailable<T>() where T : TMatter
        {
            return IsManaOfTypeAvailable(typeof(T));
        }

        public bool IsManaOfTypeAvailable(Type type)
        {
            // TODO rework to consider stateful matter as always available
            return _availableConjurers.ContainsKey(type) && _availableConjurers[type].Count > 0;
        }
    }

    public class Eris : IDisposable
    {
        readonly Library _library;
        public IErisConsulate Emanation { get; set; } // Rename to 

        CollectibleDisposable Q { get; set; }

        Queue<SerializableSpellOccurence> spellOccQueue { get; } = new();
        Queue<SerializableMatterOccurence> matterOccQueue { get; } = new();

        public IObservable<SpellOccurence> SpellOccurences => SpellStream.AsObservable();
        public IObservable<MatterOccurence> MatterOccurences => MatterStream.AsObservable();

        Subject<SpellOccurence> SpellStream { get; }
        Subject<MatterOccurence> MatterStream { get; }
        Subject<ExceptionOccurence> ExceptionStream { get; }

        public IObservable<IManaInformationProvideable> ManaProvideableObservable { get; private set; }

        public void PublishSpellOccurence(SpellOccurence spellOccurence)
        {
            SpellStream.OnNext(spellOccurence);
        }

        public void PublishExceptionOccurence(ExceptionOccurence exceptionOccurence)
        {
            ExceptionStream.OnNext(exceptionOccurence);
        }

        public void PublishMatterOccurence(MatterOccurence matterOccurence)
        {
            MatterStream.OnNext(matterOccurence);

            if (Environment.CurrentManagedThreadId != 1)
            {
                Debug.LogError($"<color=red>Left the main thread for {matterOccurence.MatterOccurenceCategory} matter type {matterOccurence.Matter.GetType().Name} at a {matterOccurence.Source.SpellSchool} spell by {matterOccurence.Source.Who.GetType()}</color>");
            }
        }

        public void PublishMessage(MessageOccurence messageOccurence)
        {
            var serializableMess = new SerializableMessageOccurence()
            {
                guid = messageOccurence.Guid,
                circumstances = messageOccurence.Circumstances,
                timestamp = messageOccurence.Timestamp.ToUnixTimeSeconds(),
                messageType = messageOccurence.MessageType,
                message = messageOccurence.Message, // * custom serializer
                exception =  new SerializableException()
                {
                    message = messageOccurence.Exception is not null ? messageOccurence.Exception.Message : "null",
                    stackTrace = messageOccurence.Exception is not null ? messageOccurence.Exception.StackTrace : "null"
                }
            };
            
            Emanation.ReceiveMessage(serializableMess);
        }

        public Eris()
        {
            Q = new CollectibleDisposable();

            SpellStream = new Subject<SpellOccurence>();
            MatterStream = new Subject<MatterOccurence>();
            ExceptionStream = new Subject<ExceptionOccurence>();

            InitializeManaStreamMystery();

            SubscribeSpellStream();
            SubscribeMatterStream();
            SubscribeExceptionStream();
        }

        void SubscribeExceptionStream()
        {
            Q += ExceptionStream
                .Do(x =>
                {
                    Debug.LogError($"<color=yellow>Message: {x.Exception.Message}</color>");
                    Debug.LogError(x.Exception.StackTrace);
                    
                    // TODO add listener to update cycle
                    // queue exception throwing there in editor
                    // so that we get it to fail properly for easy debugging
                    
                    // 1. if data structure loaded push a proper log
                    // 2. otherwise serialize to a CRASH_LOG.txt
                    
                })
                .Subscribe(occ =>
                {
                    throw occ.Exception;
                });
        }

        void SubscribeMatterStream()
        {
            Q += MatterStream
                // TODO temporary lock on high velocity matter
                .Where(occ => occ.Matter?.GetType().GetCustomAttributes(typeof(HighVelocityAttribute), true).Length == 0)
                .Select(occ =>
                {
                    try
                    {
                        var mocc = new SerializableMatterOccurence()
                        {
                            guid = occ.Guid,
                            timestamp = occ.Timestamp.ToUnixTimeSeconds(),
                            spell = GetSerializableSpell(occ.Source),
                            matter = occ.Matter,
                            matterType = occ.Matter.GetType(), // * custom serializer
                            matterOccurenceCategory = occ.MatterOccurenceCategory
                        };

                        return mocc;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Matter serialization error for {occ.Matter.GetType().Name} at {occ.Source.SpellSchool} spell by {occ.Source.Who.GetType()}");
                        throw;
                    }
                })
                .Subscribe(occ =>
                {
                    if (Emanation is null)
                    {
                        matterOccQueue.Enqueue(occ);
                        return;
                    }

                    if (matterOccQueue.Count > 0)
                    {
                        while (matterOccQueue.Count > 0)
                        {
                            Emanation.ReceiveMatterOccurence(matterOccQueue.Dequeue());
                        }
                    }

                    Emanation.ReceiveMatterOccurence(occ);
                });
        }

        void SubscribeSpellStream()
        {
            Q += SpellStream.Subscribe(occ =>
            {
                var serializableOcc = new SerializableSpellOccurence()
                {
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
                else
                {
                    if (spellOccQueue.Count > 0)
                    {
                        while (spellOccQueue.Count > 0)
                        {
                            Emanation.ReceiveSpellOccurence(spellOccQueue.Dequeue());
                        }
                    }
                }

                Emanation.ReceiveSpellOccurence(serializableOcc);
            });
        }

        // TODO 🧙🏻 wow this is complicated
        // TODO why?
        // TODO add2. and why is it here, if anything, wouldn't that be better off in library, could it be there?
        void InitializeManaStreamMystery()
        {
            // TODO 🤯 IS THIS THING USED? I HAVE NO RECOLLECTION OF WHAT IT DOES
            // CONGRATULATIONS MARIA ON WRITING THIS BLOB

            IConnectableObservable<IManaInformationProvideable> manaStream = SpellStream
                .Where(occ => occ.Source.SpellSchool
                    is SpellSchool.Looming
                    or SpellSchool.Stranding)
                .Where(occ => occ.SpellOccurenceCategory // TODO why only these occurences
                    is SpellOccurenceCategory.HasMana
                    or SpellOccurenceCategory.NoMana
                    or SpellOccurenceCategory.Forgotten)
                .Scan((false, new AvailableConjurers()), (acc, current) =>
                {
                    TStrandingSpell sourceAsStranding =
                        current.Source as TStrandingSpell ?? throw new InvalidOperationException();

                    AvailableConjurers accumulator = acc.Item2;
                    Type conjuredType = sourceAsStranding.ConjuredType;
                    bool wasManaAvailable = accumulator.IsManaOfTypeAvailable(conjuredType);

                    SpellOccurenceCategory category = current.SpellOccurenceCategory;
                    if (category is SpellOccurenceCategory.HasMana)
                    {
                        accumulator.ActivateConjurer(sourceAsStranding);
                    }
                    else
                    {
                        accumulator.DectivateConjurer(sourceAsStranding);
                    }

                    bool isManaAvailable = accumulator.IsManaOfTypeAvailable(conjuredType);

                    bool hasAnythingChanged = wasManaAvailable != isManaAvailable;

                    return (hasAnythingChanged, accumulator);
                })
                .Where(accumulator => accumulator.Item1)
                .Select(accumulator => accumulator.Item2 as IManaInformationProvideable)
                .StartWith(new AvailableConjurers())
                .Multicast(new ReplaySubject<IManaInformationProvideable>(1));

            Q += manaStream.Connect();

            ManaProvideableObservable = manaStream;

            Q += ManaProvideableObservable
                .Subscribe(info =>
                {
                    if (info.LastChangedType == null) return;
                    // Debug.Log($"next : {info.LastChangedType} {info.IsManaOfTypeAvailable(info.LastChangedType)}");
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
            // spell.whosName = source.Who is MonoBehaviour
            //     ? $"{(source.Who as MonoBehaviour).gameObject.name}'s {source.Who.GetType().Name}"
            //     : source.Who.GetType().Name;
            spell.whosName = GetWho(source).WhosType.Name;
            spell.hasMana = source.HasMana;

            return spell;
        }

        SerializableStranding GetSerializableStranding(TSpell source)
        {
            TStrandingSpell strand = source as TStrandingSpell;

            Debug.Assert(strand != null, nameof(strand) + " != null");

            SerializableStranding serializableStranding = new SerializableStranding()
            {
                spellSchool = SpellSchool.Stranding,
                conjuredType = strand.ConjuredType,
                Who = GetWho(source)
            };

            return serializableStranding;
        }

        private SerializableLooming GetSerializableLooming(TSpell source)
        {
            TBindingSpell binding = source as TBindingSpell;
            TStrandingSpell stranding = source as TStrandingSpell;

            Debug.Assert(binding != null, nameof(binding) + " != null");
            Debug.Assert(stranding != null, nameof(stranding) + " != null");
            SerializableLooming looming = new SerializableLooming()
            {
                spellSchool = SpellSchool.Looming,
                ingredients = GetSerializableIngredients(binding),
                hasMana = binding.HasMana,
                conjuredType = stranding.ConjuredType,
                Who = GetWho(source)
            };

            return looming;
        }

        private SerializableWeaving GetSerializableWeaving(TSpell source)
        {
            TBindingSpell binding = source as TBindingSpell;

            Debug.Assert(binding != null, nameof(binding) + " != null");
            SerializableWeaving weaving = new SerializableWeaving()
            {
                spellSchool = SpellSchool.Weaving,
                ingredients = GetSerializableIngredients(binding),
                hasMana = binding.HasMana,
                Who = GetWho(source)
            };

            return weaving;
        }

        Who GetWho(TSpell source)
        {
            Type whosType = source.Who.GetType();

            string parentGameObjectName = null;
            if (source.Who is MonoBehaviour monoWho)
            {
                // try
                // {
                    parentGameObjectName = monoWho.gameObject.name;
                // }
                // catch (Exception e)
                // {
                //     Debug.Log($"<color=red>Illegally attempted to access gameObject info while not bein on the main thread.</color>");
                //     parentGameObjectName = "unknown error";
                // }
            }

            return new Who()
            {
                WhosType = whosType,
                ParentGameObjectName = parentGameObjectName
            };
        }

        [Obsolete] // TODO there is a problem with that, there are no longer ingredients list
        private Dictionary<string, bool> GetSerializableIngredients(TBindingSpell binding)
        {
            return binding
                .SatisfiedRequirements
                .Select(kvp => new KeyValuePair<string, bool>(
                        key: kvp.Key.Name,
                        value: kvp.Value) // oops
                )
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ;
            // return binding
            //     .SatisfiedRequirements
            //     .Select(kvp => new KeyValuePair<string, bool>(
            //         key: kvp.Key.Name,
            //         value: kvp.Value) // oops
            //     )
            //     .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public void Dispose()
        {
            Debug.Log($"<color=blue>Bye Eris!</color>");

            Q.Dispose();
        }

        void Print(string color, string head, string msg, params object[] args)
        {
            string text = string.Format(msg, args);
        }
    }
}