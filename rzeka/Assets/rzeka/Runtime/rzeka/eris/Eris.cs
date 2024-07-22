using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json;
using UnityEngine;

namespace Rzeka
{
    public class Eris : IDisposable
    {
        readonly Library _library;
        public IErisConsulate Emanation { get; set; } // Rename to 

        CollectibleDisposable Q { get; set; }

        Queue<ISerializableSpellOccurence> spellOccurenceQueue { get; } = new();
        Queue<ISerializableMatterOccurence> matterOccurenceQueue { get; } = new();
        Queue<SerializableMessageOccurence> messageOccurenceQueue { get; } = new();

        public IObservable<SpellOccurence> SpellOccurences => SpellStream.AsObservable();
        public IObservable<MatterOccurence> MatterOccurences => MatterStream.AsObservable();

        Subject<SpellOccurence> SpellStream { get; } = new();
        Subject<MatterOccurence> MatterStream { get; } = new();
        Subject<ExceptionOccurence> ExceptionStream { get; } = new();
        Subject<MessageOccurence> MessageStream { get; } = new();

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
            if (Environment.CurrentManagedThreadId != 1)
            {
                Debug.LogError($"<color=red>Left the main thread for {matterOccurence.MatterOccurenceCategory} matter type {matterOccurence.Matter.GetType().Name} at a {matterOccurence.Source.SpellSchool} spell by {matterOccurence.Source.Who.GetType()}</color>");
            }
            
            MatterStream.OnNext(matterOccurence);
        }

        public void PublishMessage(MessageOccurence messageOccurence)
        {
            if (Environment.CurrentManagedThreadId != 1)
            {
                Debug.LogError($"<color=red>Left the main thread for Message: {messageOccurence.Message}");
            }
            
            // TODO Rework other serializable occurence baking like that
            MessageStream.OnNext(messageOccurence);
        }

        public Eris()
        {
            Q = new CollectibleDisposable();

            InitializeManaStreamMystery();

            SubscribeSpellStream();
            SubscribeMatterStream();
            SubscribeExceptionStream();
            SubscribeMessageStream();
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
                .Select<MatterOccurence, ISerializableMatterOccurence>(occ =>
                {
                    try
                    {
                        switch (occ.MatterOccurenceCategory)
                        {
                            case MatterOccurenceCategory.Shaped:
                                return new SerializableShapedMatter(
                                    occ.Guid,
                                    occ.Timestamp.ToUnixTimeSeconds(),
                                    GetSerializableSpell(occ.Source),
                                    occ.Matter.GetType(), // * custom serializer
                                    occ.Matter
                                );
                            case MatterOccurenceCategory.Received:
                                return new SerializableReceivedMatter(
                                    occ.Guid,
                                    occ.Timestamp.ToUnixTimeSeconds(),
                                    occ.Matter.GetType(), // * custom serializer
                                    occ.Matter.Guid);
                            default:
                                throw new Exception($"Unhandled matter occurence category {occ.MatterOccurenceCategory}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Matter serialization error for {occ.Matter.GetType().Name} at {occ.Source.SpellSchool} spell by {occ.Source.Who.GetType()}");
                        return null;
                    }
                })
                .Subscribe(occ =>
                {
                    if (Emanation is null)
                    {
                        matterOccurenceQueue.Enqueue(occ);
                        return;
                    }

                    if (matterOccurenceQueue.Count > 0)
                    {
                        while (matterOccurenceQueue.Count > 0)
                        {
                            Emanation.ReceiveMatterOccurence(matterOccurenceQueue.Dequeue());
                        }
                    }

                    Emanation.ReceiveMatterOccurence(occ);
                });
        }

        void SubscribeSpellStream()
        {
            Q += SpellStream
                .Where(x => x.SpellOccurenceCategory 
                    is SpellOccurenceCategory.Created or SpellOccurenceCategory.Forgotten)
                .Subscribe(occ =>
            {
                ISerializableSpellOccurence serializableSpellOccurence;
                
                if (occ.SpellOccurenceCategory is SpellOccurenceCategory.Created)
                {
                    serializableSpellOccurence = new SerializableCreatedSpellOccurence(
                        occ.Guid,
                        occ.Timestamp.ToUnixTimeSeconds(),
                        GetSerializableSpell(occ.Source));
                }
                else
                {
                    serializableSpellOccurence = new SerializableOtherSpellOccurence(
                        occ.Guid,
                        occ.Timestamp.ToUnixTimeSeconds(),
                        occ.SpellOccurenceCategory,
                        occ.Source.Guid);
                }

                if (Emanation is null)
                {
                    spellOccurenceQueue.Enqueue(serializableSpellOccurence);
                    return;
                }
                else
                {
                    if (spellOccurenceQueue.Count > 0)
                    {
                        while (spellOccurenceQueue.Count > 0)
                        {
                            Emanation.ReceiveSpellOccurence(spellOccurenceQueue.Dequeue());
                        }
                    }
                }

                Emanation.ReceiveSpellOccurence(serializableSpellOccurence);
            });
        }

        void SubscribeMessageStream()
        {
            Q += MessageStream.Subscribe(occ =>
            {
                SerializableMessageOccurence serializableMessage = SerializableMessageOccurence.FromMessageOccurence(occ);

                if (Emanation is null)
                {
                    messageOccurenceQueue.Enqueue(serializableMessage);
                    return;
                }
                else
                {
                    if (messageOccurenceQueue.Count > 0)
                    {
                        while (messageOccurenceQueue.Count > 0)
                        {
                            Emanation.ReceiveMessage(messageOccurenceQueue.Dequeue());
                        }
                    }
                }

                Emanation.ReceiveMessage(serializableMessage);
            });
        }

        // TODO 🧙🏻 wow this is complicated
        // TODO why?
        // TODO add2. and why is it here, if anything, wouldn't that be better off in library, could it be there?
        // TODO THIS IS REALLY ITCHY
        // TODO TOTES SHOULD *NOT* BE HERE
        // TODO mana stream is referenced by TBindingSpell !!!! ridiculous
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