using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka;
public class Eris : IDisposable
{
    CollectibleDisposable Q { get; set; }

    public IObservable<SpellOccurence> SpellOccurences => SpellStream.AsObservable();
    public IObservable<MatterOccurence> MatterOccurences => MatterStream.AsObservable();

    ReplaySubject<ISerializableSpellOccurence> SerializableSpellStream { get; } =
        new(TimeSpan.FromMinutes(1));
    ReplaySubject<ISerializableMatterOccurence> SerializableMatterStream { get; } =
        new(TimeSpan.FromMinutes(1));
    ReplaySubject<SerializableMessageOccurence> SerializableMessageStream { get; } =
        new(TimeSpan.FromMinutes(1));


    public IObservable<ISerializableSpellOccurence> SerializableSpellOccurences =>
        SerializableSpellStream.AsObservable();
    public IObservable<ISerializableMatterOccurence> SerializableMatterOccurences =>
        SerializableMatterStream.AsObservable();
    public IObservable<SerializableMessageOccurence> SerializableMessageOccurences =>
        SerializableMessageStream.AsObservable();

    Subject<SpellOccurence> SpellStream { get; } = new();
    Subject<MatterOccurence> MatterStream { get; } = new();
    Subject<MessageOccurence> MessageStream { get; } = new();

    public void PublishSpellOccurence(SpellOccurence spellOccurence)
    {
        SpellStream.OnNext(spellOccurence);
    }

    public void PublishMatterOccurence(MatterOccurence matterOccurence)
    {
        if (Environment.CurrentManagedThreadId != 1)
        {
            PublishMessage(new MessageOccurence
            {
                Guid = Guid.NewGuid(),
                Timestamp = DateTimeOffset.Now,
                RzekaMessageType = RzekaMessageType.Horror,
                Message =
                    $"Off-thread matter: {matterOccurence.Matter.GetType().Name} ({matterOccurence.MatterOccurenceCategory}) via {matterOccurence.Source.SpellSchool} spell by {matterOccurence.Source.Who.GetType().Name}",
            });
        }

        MatterStream.OnNext(matterOccurence);
    }

    public void PublishMessage(MessageOccurence messageOccurence)
    {
        MessageStream.OnNext(messageOccurence);
    }

    public string Name { get; }

    // Optional user hook invoked after the boundary whispers an unhandled source error.
    // Set at river creation via Spring.Create(name, onUnhandledSourceError: ...).
    // If the callback throws, the throw propagates as OnError into the river's pipeline
    // (Stream<T>'s default observer rethrows on the source thread — i.e. crash loudly).
    internal Action<ISpell, Exception>? OnUnhandledSourceError { get; set; }

    public Eris(string name)
    {
        Name = name;
        Q = new CollectibleDisposable();

        SubscribeSpellStream();
        SubscribeMatterStream();
        SubscribeMessageStream();
    }

    public void Dispose()
    {
        PublishMessage(new MessageOccurence
        {
            Guid = Guid.NewGuid(),
            Timestamp = DateTimeOffset.Now,
            RzekaMessageType = RzekaMessageType.Hint,
            Message = "༼ つ ◕_◕ ༽つ Bye Eris!",
        });

        Q.Dispose();
    }

    void SubscribeMatterStream()
    {
        Q += MatterStream
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
                                occ.Matter.GetType(), // * custom serializer
                                occ.Source.Guid,
                                occ.Matter
                            );
                        case MatterOccurenceCategory.Received:
                            return new SerializableReceivedMatter(
                                occ.Guid,
                                occ.Timestamp.ToUnixTimeSeconds(),
                                occ.Source.Guid,
                                occ.Matter.Guid
                            );
                        default:
                            throw new Exception(
                                $"Unhandled matter occurence category {occ.MatterOccurenceCategory}"
                            );
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            })
            .Subscribe(occ =>
            {
                if (occ is null)
                    return;
                SerializableMatterStream.OnNext(occ);
            });
    }

    void SubscribeSpellStream()
    {
        Q += SpellStream
            .Where(x =>
                x.SpellOccurenceCategory
                    is SpellOccurenceCategory.Created
                        or SpellOccurenceCategory.Forgotten
                        or SpellOccurenceCategory.HasMana
                        or SpellOccurenceCategory.NoMana
                        or SpellOccurenceCategory.Wispd
            )
            .Subscribe(occ =>
            {
                ISerializableSpellOccurence serializableSpellOccurence;

                if (occ.SpellOccurenceCategory is SpellOccurenceCategory.Created)
                {
                    serializableSpellOccurence = new SerializableCreatedSpellOccurence(
                        occ.Guid,
                        occ.Timestamp.ToUnixTimeSeconds(),
                        GetSerializableSpell(occ.Source)
                    );
                }
                else if (occ.SpellOccurenceCategory is SpellOccurenceCategory.Wispd)
                {
                    serializableSpellOccurence = new SerializableWispdSpellOccurence(
                        occ.Guid,
                        occ.Timestamp.ToUnixTimeSeconds(),
                        occ.Source.Guid,
                        SerializableException.FromException(occ.Exception)
                    );
                }
                else
                {
                    serializableSpellOccurence = new SerializableOtherSpellOccurence(
                        occ.Guid,
                        occ.Timestamp.ToUnixTimeSeconds(),
                        occ.SpellOccurenceCategory,
                        occ.Source.Guid
                    );
                }

                SerializableSpellStream.OnNext(serializableSpellOccurence);
            });
    }

    void SubscribeMessageStream()
    {
        Q += MessageStream.Subscribe(occ =>
        {
            SerializableMessageOccurence serializableMessage =
                SerializableMessageOccurence.FromMessageOccurence(occ);

            SerializableMessageStream.OnNext(serializableMessage);
        });
    }

    // ? move this to the scroll so it wont have to be created each time if that makes sense
    ISerializableSpell GetSerializableSpell(ISpell source)
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
        else if (source.SpellSchool is SpellSchool.Plucking)
        {
            spell = GetSerializablePlucking(source);
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

    SerializableStranding GetSerializableStranding(ISpell source)
    {
        IStrandingSpell strand = source as IStrandingSpell;

        Debug.Assert(strand != null, nameof(strand) + " != null");

        SerializableStranding serializableStranding = new SerializableStranding()
        {
            spellSchool = SpellSchool.Stranding,
            conjuredType = strand.ConjuredType,
            Who = GetWho(source),
        };

        return serializableStranding;
    }

    SerializableStranding GetSerializablePlucking(ISpell source)
    {
        IStrandingSpell strand = source as IStrandingSpell;

        Debug.Assert(strand != null, nameof(strand) + " != null");

        return new SerializableStranding()
        {
            spellSchool = SpellSchool.Plucking,
            conjuredType = strand.ConjuredType,
            Who = GetWho(source),
        };
    }

    private SerializableLooming GetSerializableLooming(ISpell source)
    {
        IBindingSpell binding = source as IBindingSpell;
        IStrandingSpell stranding = source as IStrandingSpell;

        Debug.Assert(binding != null, nameof(binding) + " != null");
        Debug.Assert(stranding != null, nameof(stranding) + " != null");
        SerializableLooming looming = new SerializableLooming()
        {
            spellSchool = SpellSchool.Looming,
            ingredients = GetSerializableIngredients(binding),
            hasMana = binding.HasMana,
            conjuredType = stranding.ConjuredType,
            Who = GetWho(source),
        };

        return looming;
    }

    private SerializableWeaving GetSerializableWeaving(ISpell source)
    {
        IBindingSpell binding = source as IBindingSpell;

        Debug.Assert(binding != null, nameof(binding) + " != null");
        SerializableWeaving weaving = new SerializableWeaving()
        {
            spellSchool = SpellSchool.Weaving,
            ingredients = GetSerializableIngredients(binding),
            hasMana = binding.HasMana,
            Who = GetWho(source),
        };

        return weaving;
    }

    Who GetWho(ISpell source) => new Who { WhosType = source.Who.GetType() };

    // TODO there is a problem with that, there are no longer ingredients list
    private Dictionary<string, bool> GetSerializableIngredients(IBindingSpell binding)
    {
        return binding
            .SatisfiedRequirements.Select(kvp => new KeyValuePair<string, bool>(
                key: kvp.Key.Name,
                value: kvp.Value
            ) // oops
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
}

