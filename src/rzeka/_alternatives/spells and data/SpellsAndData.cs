using Rzeka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Joins;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace SpellsAndData
{
    public interface TMatter
    {
        Guid Guid { get; set; }
        Guid[] Circumstances { get; set; }
        string Description { get; }

        public void SetCircumstances(params TMatter[] circumstances)
        {
            Circumstances = circumstances.Select(x => x.Guid).ToArray();
        }
    }

    public interface IRequest : TMatter { }
    public interface IResponse<T> : TMatter where T : IRequest
    {
        T Request { get; }
    }

    public interface TAllowingMultipleSources<T> where T : TMatter
    {
        public IObservable<T> CombineSourcesPattern(params IObservable<T>[] sources)
            => Observable.Merge(sources);
    }

    public interface ICacheLast : IHasDefaultValue
    {
        //WindowSize 
    }

    public interface ICacheBuffer
    {
        int BufferSize { get; }

    }

    public interface IHasDefaultValue
    {
        void SetToDefaultValues();
        bool TreasureData { get; } // TODO Q
    }

    public abstract class LoomingMono : MonoBehaviour
    {
        protected CollectibleDisposable q { get; set; } = new();

        protected virtual void OnEnable()
        {
            q ??= new();
        }

        protected virtual void OnDisable()
        {
            q.Dispose();
        }
    }

    public interface IRzeka
    {

        // ! Pure Givers 0-

        void Pluck<T>(object who, T newValue, params TMatter[] circumstances) where T : TMatter;
        // This one is to talk about since
        void Pluck<T>(object who, Func<T, T> update, params TMatter[] circumstances) where T : TMatter;
        // Here circumstances must be provided manually inside IObservable definition

        // ! Pure Takers AB+

        IDisposable Weave<T>(object who, IObserver<T> taker) where T : TMatter;
        IDisposable Weave<T>(object who, Func<IObservable<T>, IDisposable> takerSpell) where T : TMatter;
        IDisposable Weave<T, Y>(object who, Func<Pattern<T, Y>, IDisposable> takerSpell) where T : TMatter;

        // ! Contractors
        // So far I see them as dealing with Request / Response type of events

        // Answering
        IDisposable Answer<Tin, Tout>(object who, Func<Tin, IObservable<Tout>> onQuestion) where Tin : IRequest where Tout : IResponse<Tin>;
        IDisposable Answer<Tin, Tout>(object who, Func<IObservable<Tin>, IObservable<Tout>> onQuestion) where Tin : IRequest where Tout : IResponse<Tin>;

        // Requesting
        IDisposable Ask<Tin, Tout>(object who, Tin question, IObserver<Tout> answerObserver) where Tin : IRequest where Tout : IResponse<Tin>;
        IDisposable Ask<Tin, Tout>(object who, IObservable<Tin> questionStream, IObserver<Tout> answerObserver) where Tin : IRequest where Tout : IResponse<Tin>;
        IDisposable Ask<Tin, Tout>(object who, IObservable<Tin> questionStream, Func<IObservable<Tout>, IDisposable> onAnswerStream) where Tin : IRequest where Tout : IResponse<Tin>;

        // Notice it could quickly get nasty this way, there should be another chaining option
        //[Obsolete] IDisposable Ask<Tin, Y, Tout>(object who, Func<IObservable<Y>, IObservable<Tin>> questionStream, Func<IObservable<Tout>> onAnswerStream);

        // ! Looms
        // They are actually close to givers than to the takers even though they do seem to "observe" other streams
        // But they don't really stop to subscribe, they transform different streams into a new one

        // They are also somehow contracts

        // In this cases it must be assumed that the user assignes proper circumstances to the elements in stream
        IDisposable Loom<T>(object who, Func<IObservable<T>> observable); // ! This was originaly a 'pluck' since it only provides
        IDisposable Loom<T, Tout>(object who, Func<IObservable<T>, IObservable<Tout>> spell);
        IDisposable Loom<T, Y, Tout>(object who, Func<Pattern<T, Y>, IObservable<Tout>> spell);
        IDisposable Loom<T, Y, U, Tout>(object who, Func<Pattern<T, Y, U>, IObservable<Tout>> spell);
        IDisposable Loom<T, Y, U, X, Tout>(object who, Func<Pattern<T, Y, U, X>, IObservable<Tout>> spell);
        // TODO And so on
    }

    public class Rzeka
    {
        /// <summary>
        /// Underlyin assumption is that for each type of Matter there is it's own but a single stream.
        /// </summary>
        Dictionary<Type, object> Subjects;
        Dictionary<Type, object> Dreams;

        Subject<SharedDreamHappening> HappeningsStream = new();

        public IDisposable Loom<T>(object who, Func<IObservable<T>> observable)
        {


            return Disposable.Empty;
        }

        /// <summary>
        /// Once given an observable stream of the type they are interested in they will produce a new stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="who"></param>
        /// <param name="spell"></param>
        /// <returns></returns>
        public IDisposable Loom<T, Tout>(object who, Func<IObservable<T>, IObservable<Tout>> spell)
            where T : TMatter
            where Tout : TMatter
        {
            LoomDream<Tout> loomDream = null;
            Type outKey = typeof(Tout);
            if (Dreams.ContainsKey(outKey))
            {
                loomDream = Dreams[outKey] as LoomDream<Tout>;
            }
            else
            {
                LoomSpell<T, Tout> loomSpell = new(who, spell, null);
                //loomDream = new LoomDream<Tout>(loomSpell);
                Dreams.Add(outKey, loomDream);
            }


            IDisposable spellReversal = null;
            Type keyT = typeof(T);

            if (Subjects.ContainsKey(keyT))
            {
                var stream = Subjects[keyT] as ISubject<T>;

                IObservable<Tout> invocation = spell(stream.AsObservable());

                // this invocation should now be saved as a new available stream
                spellReversal = HookUp(invocation);
            }
            else
            {
                // todo store spell for future invocation when its ingredient appears
                // todo on each appearing ingreedient check if there were spells waiting for it
            }

            // if such stream

            return spellReversal;
        }

        public IDisposable Loom<T, Y, Tout>(object who, Func<Pattern<T, Y>, IObservable<Tout>> spell)
        {
            dynamic val = spell;

            //val.Innvoke();

            //Pattern<T, Y> asdf


            return Disposable.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="Tout"></typeparam>
        /// <param name="invocation">A new observable presented to the stream</param>
        /// <returns>A hookup subscription of a subject corresponding to Tout type, Dispoosable can be used to end the hookup</returns>
        IDisposable HookUp<Tout>(IObservable<Tout> invocation)
            where Tout : TMatter
        {
            Tout value = default;
            SubjectBase<Tout> subject = null;
            Type keyOut = typeof(Tout);

            if (Subjects.ContainsKey(keyOut))
            {
                if (Subjects[keyOut] is TAllowingMultipleSources<Tout>)
                {
                    subject = Subjects[keyOut] as SubjectBase<Tout>;

                    // TODO this would be too easy, must verify
                    // TODO simple wrapping of previous subject with its observers
                    var combinedObservable = (value as TAllowingMultipleSources<Tout>)
                        .CombineSourcesPattern(subject.AsObservable(), invocation);

                    subject = Subject.Create<Tout>(
                        observer: subject,
                        observable: combinedObservable) as SubjectBase<Tout>;

                    Subjects[keyOut] = subject;
                }
                else
                {
                    // Throw some error that someone already exclusively provides that type
                }
            }
            else
            {
                bool hasDefaultValue = false;

                if (typeof(IHasDefaultValue).IsAssignableFrom(keyOut))
                {
                    hasDefaultValue = true;
                    (value as IHasDefaultValue).SetToDefaultValues();
                }

                if (typeof(ICacheLast).IsAssignableFrom(keyOut))
                {
                    subject = new BehaviorSubject<Tout>(value);
                }
                else if (typeof(ICacheBuffer).IsAssignableFrom(keyOut))
                {
                    int bufferSize = (value as ICacheBuffer).BufferSize;
                    subject = new ReplaySubject<Tout>(bufferSize);
                    if (hasDefaultValue) subject.OnNext(value);
                }
                else
                {
                    subject = new Subject<Tout>();
                    if (hasDefaultValue) subject.OnNext(value);
                }

                Subjects[keyOut] = subject;
            }

            // TODO Adding wrapping down of subjects wh en no listeners are present?
            // ! Subject is getting hooked up with a new observable provided to the stream
            IObserver<Tout> observer = subject;

            IDisposable subscription = invocation.Subscribe(observer);

            IDisposable spellReversal = Disposable.Create<SubjectBase<Tout>>(
                    state: subject,
                    dispose: sub =>
                    {
                        subscription.Dispose();

                        if (sub.HasObservers == false)
                        {
                            sub.Dispose();
                        }
                    });

            return spellReversal;
        }

        public enum HappeningType { NewObservable, NewObserver }
        public abstract class SharedDreamHappening
        {
            public HappeningType HappeningType { get; private set; }
            public Type AffectedType { get; private set; }

            public SharedDreamHappening(HappeningType happeningType, Type affectedType)
            {
                HappeningType = happeningType;
                AffectedType = affectedType;
            }
        }

        public class IntroductionHappening<T> : SharedDreamHappening
        {
            public IObservable<T> IntroducedObservable { get; }

            public IntroductionHappening(IObservable<T> introducedObservable) : base(HappeningType.NewObservable, typeof(T))
            {
                IntroducedObservable = introducedObservable;
            }
        }

        public enum DreamType { None, Suspended, Receiving, Giving, Sharing }

        public abstract class DreamBase<T> : IDisposable where T : TMatter
        {
            public ISubject<T> subject;

            public DreamBase()
            {
                T value = default;
                Type key = typeof(T);

                bool hasDefaultValue = false;

                if (typeof(IHasDefaultValue).IsAssignableFrom(key))
                {
                    hasDefaultValue = true;
                    (value as IHasDefaultValue).SetToDefaultValues();
                }

                if (typeof(ICacheLast).IsAssignableFrom(key))
                {
                    subject = new BehaviorSubject<T>(value);
                }
                else if (typeof(ICacheBuffer).IsAssignableFrom(key))
                {
                    int bufferSize = (value as ICacheBuffer).BufferSize;
                    subject = new ReplaySubject<T>(bufferSize);
                    if (hasDefaultValue) subject.OnNext(value);
                }
                else
                {
                    subject = new Subject<T>();
                    if (hasDefaultValue) subject.OnNext(value);
                }
            }

            public virtual void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        public class LoomDream<T> : DreamBase<T> where T : TMatter
        {
            public List<LoomSpellBase<T>> spells;

            public IObservable<T> Observable;

            public LoomDream(Subject<SharedDreamHappening> HappeningsStream, LoomSpellBase<T> startingSpell) : base()
            {
                spells = new() { startingSpell };

                // todo unregister 
                IDisposable listenForIntroductions = HappeningsStream
                    .Where(_ => spells.Any(spell => spell.IsActive == false))
                    .Where(happening => happening.HappeningType == HappeningType.NewObservable)
                    .Select(happening =>
                    {
                        return spells
                            .ToObservable()
                            .Where(spell =>
                            {
                                return spell.requirements.ContainsKey(happening.AffectedType)
                                && spell.requirements[happening.AffectedType].isProvided is false;
                            })
                            .Select(spell =>
                            {
                                //spell.
                                return spell;
                            });
                    }).Subscribe();
            }

            public void AddSpell(LoomSpellBase<T> spell)
            {
            }

            public override void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        public abstract class SpellBase
        {
            public readonly object who;
            public readonly Dictionary<Type, (bool isProvided, object observable)> requirements;

            public abstract bool IsActive { get; protected set; }
            public bool CanBeCast
            {
                get => IsActive == false && requirements.All(kvp => kvp.Value.isProvided == true);
            }

            public SpellBase(object who, params Type[] requiredTypes)
            {
                this.who = who;
                requirements = new Dictionary<Type, (bool, object)>(requiredTypes.Length);
                foreach (var r in requiredTypes)
                {
                    requirements.Add(r, (false, null));
                }
            }

            public bool IsInterested(Type type) => requirements.ContainsKey(type);

            public void Provide<T>(IObservable<T> observable)
            {
                //foreach (var req in requirements.Where(r => r.Key == typeof(T)).)
                //{
                //    req.Value = (true, observable);
                //}
            }

        }

        public abstract class LoomSpellBase<Tout> : SpellBase where Tout : TMatter
        {
            public Type ProvidedType { get => typeof(Tout); }

            public override bool IsActive { get; protected set; }

            public LoomSpellBase(object who, params Type[] requiredTypes) : base(who, requiredTypes){ }

            public abstract IObservable<Tout> Cast(params object[] parameters);
        }

        public class LoomSpell<Tout> : LoomSpellBase<Tout> where Tout : TMatter
        {
            public Func<IObservable<Tout>> spell;

            public LoomSpell(object who, Func<IObservable<Tout>> spell, params Type[] requiredTypes)
                : base(who, requiredTypes)
            {
                this.spell = spell;
            }


            public override IObservable<Tout> Cast(params object[] parameters)
            {
                //if (parameters.Length != requiredTypes.Length)
                //{
                //    // throw an error
                //}

                IObservable<Tout> castResult = null;

                if (castResult is null) throw new Exception("sonmething went wrong");

                return castResult;
            }
        }

        public class LoomSpell<T, Tout> : LoomSpellBase<Tout> where Tout : TMatter
        {
            public Func<IObservable<T>, IObservable<Tout>> spell { get; set; }

            public LoomSpell(object who, Func<IObservable<T>, IObservable<Tout>> spell, params Type[] requiredTypes)
                : base(who, requiredTypes)
            {
                this.spell = spell;
            }

            public override IObservable<Tout> Cast(params object[] parameters)
            {
                throw new NotImplementedException();
            }
        }

        public class LoomSpell<T, Y, Tout> : LoomSpellBase<Tout> where Tout : TMatter
        {
            public Func<Pattern<T, Y>, IObservable<Tout>> spell { get; set; }

            public LoomSpell(object who, Func<Pattern<T, Y>, IObservable<Tout>> spell, params Type[] requiredTypes)
                : base(who, requiredTypes)
            {
                this.spell = spell;
            }

            public override IObservable<Tout> Cast(params object[] parameters)
            {
                throw new NotImplementedException();
            }
        }
    }
}
