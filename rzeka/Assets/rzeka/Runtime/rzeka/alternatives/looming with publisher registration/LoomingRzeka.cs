/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Joins;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using UnityEngine;

/*

! if exceptions are gone, check if you did some weird typecasting of the
! observable generic type <T>

This version information:

## 🐌 Does not contain neither WhoObserver nor IWeaveable

- WhoObserver served as a wrapper for the provided observers (now handled in Weave)
- IWeaveable injected sideeffects into onnext items, now aswell injected
as a sidefffect during looming

## 🐌 Does not contain central stream

- Instead of a central stream that everyone can publish to, the root
contains only a net of interests 
- Possible because side effects are now injected during weaving and looming

*/

namespace Looming
{
    /* 🌊 ---- ---- */

    public interface IMatter
    {
        string Description => "missing description";
    }

    public interface TAllowingMultipleSources<T> where T : IMatter
    {
        static IObservable<T> CombineSourcesPattern(IEnumerable<IObservable<T>> sources)
            => Observable.Merge(sources);
    }

    /*
     * Boith can be observable answer question or specific answer
     * overload to determine that all answers are interestinh
     **/
    public interface IQuestion<T> where T : IMatter, IAnswer
    {
        bool IsItMyAnswer(T answer) => answer.QuestionSource == this;
    }

    public interface IAnswer
    {
        public object QuestionSource { get; }
    }

    //
    // ⛺ ─── Thoughts ───────────────────────────────────────────────────
    //
    #region Thoughts

    //public abstract class Matter
    //{ 
    //    public abstract string Description { get; }
    //}

    public abstract class ThoughtBase : IDisposable
    {
        protected ThoughtBase[] _circumstances;
        protected object _who;

        //
        // ⛺ ─── Properties ───────────────────────────────────────────────────
        //
        #region Properties

        internal object Who
        {
            get
            {
                if (_who is null) throw new Exception("<context> was not initialized");
                return _who;
            }
            set
            {
                if (_who is null) _who = value;
                else throw new Exception("Context has already been set for this thought.");
            }
        }

        internal ThoughtBase[] Circumstances
        {
            get
            {
                if (_circumstances is null) throw new Exception("<circumnstances> were not initialized");
                return _circumstances;
            }
            set
            {
                if (_circumstances is null) _circumstances = value;
                else throw new Exception("Circumstances were already set for this thought.");
            }
        }

        #endregion // ---------------------------------- Properties -------------------------


        protected void Initialize(object who, ThoughtBase[] circumstances)
        {
            Who = who;
            Circumstances = circumstances;
        }

        public virtual void Dispose()
        {
            _circumstances = null;
            _who = null;
        }
    }

    public class Dream<M> : ThoughtBase, IObservable<M>
        where M : struct, IMatter
    {
        private IObservable<M> _matter;
        public IObservable<M> Matter => _matter;

        public void Initialize(object who, IObservable<M> matter, params ThoughtBase[] circumstances)
        {
            base.Initialize(who, circumstances);

            _matter = matter;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public IDisposable Subscribe(IObserver<M> observer)
        {
            //return _matter.Subscribe(observer);
            throw new NotImplementedException();
        }
    }

    public class Question<Q, A> : ThoughtBase
        where Q : struct, IMatter
        where A : struct, IMatter
    {
        public Q question;
        public A answer;

        public void Initialize(object who, Q question, A answer, params ThoughtBase[] circumstances)
        {
            base.Initialize(who, circumstances);

            this.question = question;
            this.answer = answer;
        }
    }

    public class ThoughtFactory
    {
        public Dream<M> Think<M>(object who, M matter, params ThoughtBase[] circumstances)
            where M : struct, IMatter
        {
            throw new NotImplementedException();
            //Dream<M> thought = new();
            //thought.Initialize(who, matter, circumstances);
            //return thought;
        }
    }

    #endregion // ---------------------------------- Thoughts -------------------------


    //
    // ⛺ ─── Looms ───────────────────────────────────────────────────
    //
    #region Looms

    public abstract class LoomBase
    {
        private readonly object observable;

        virtual internal object Observable => observable;

        internal LoomBase(object observable)
        {
            this.observable = observable ?? throw new ArgumentNullException(nameof(observable));
        }
    }

    public class MultiLoom : LoomBase
    {
        List<LoomBase> loomBases;

        public MultiLoom(object observable) : base(observable)
        {
        }
    }

    // TODO Should looms loom matter or thoughts?
    public class Loom<M> : LoomBase
        where M : struct, IMatter
    {
        internal Loom(IObservable<M> observable)
            : base(observable)
        {
        }

        // public IObservable<T> GetObservable()
        // {
        //     return subject;
        // }

        // ? if you provide it with an observable there is no guarantee pluck can be used
        // * maybe Plucking is 
        // public void Pluck(U matter, params ThoughtBase[] circumstances)
        // {
        //     T thought = new T();

        //     // ? Throw a warning when circumstances are null
        //     thought.Initialize(who, matter, circumstances);

        //     rzeka.Pluck<T>(thought);
        // }
    }

    public class Loom<Q, A> : LoomBase
        where Q : struct, IMatter
        where A : struct, IMatter
    {
        /* ⭐ ---- ---- */

        private readonly object who;

        // TODO This could be used as a place to create a default HOT observable
        // TODO if a custom observable was not specified
        internal Loom(Func<Q, IObservable<A>> observable)
            : base(observable)
        {
        }

        /* ---- ---- 🌠 */
    }

    #endregion // ---------------------------------- Looms -------------------------


    //
    // ⛺ ─── Rzeka ───────────────────────────────────────────────────
    //
    #region Rzeka

    // TODO IS SUBSCRIBE PASSED UP?

    public class LoomingRzeka : MonoBehaviour
    {
        /* ⭐ ---- ---- */

        // * removal of a central stream 
        [Obsolete] readonly Subject<ThoughtBase> _rzeka = new();

        readonly Dictionary<Type, List<LoomBase>> _net = new();

        readonly Dictionary<Type, object> _web = new();

        readonly ThoughtFactory _factory = new();

        IDisposable _riverWisp;

        void Awake()
        {
            // -------------

            _riverWisp = _rzeka
                .Subscribe(
                    onNext: e =>
                    {

                        Type key = e.GetType();

                    }
                );

            // -------------
        }

        void OnDestroy()
        {
            // -------------

            _riverWisp.Dispose();

            // -------------
        }

        public IObservable<M> Weave<M>(object who)
            where M : struct, IMatter
        {
            Type key = typeof(Dream<M>);

            IObservable<M> weave;

            if (typeof(M) is TAllowingMultipleSources<M>)
            {
                IEnumerable<IObservable<M>> sources = _net[key]
                    .Select(loom => loom.Observable as IObservable<M>);

                weave = TAllowingMultipleSources<M>.CombineSourcesPattern(sources);
            }
            else
            {
                weave = _net[key][0].Observable as IObservable<M>;
            }

            return Observable
                .Create<M>(subscribe: observer =>
                {
                    // TODO Who observer becomes unnecessary
                    // I have access to who here
                    // In this case weaveable also becomes redundant
                    // WhoObserver<ThoughtBase> whoObserver = new(observer, who);

                    // TODO 1. Publish an information on new subscriber
                    // TODO 2. Wrap the returned disposable with unsubscribe information

                    return weave.Subscribe(observer);
                });
        }

        public IObservable<A> Weave<Q, A>(object who, Q question)
            where Q : struct, IMatter
            where A : struct, IMatter
        {
            //Type key = typeof(A); // !!!! ERROR
            //var observable = _net[key].Observable as Func<Q, IObservable<A>>;
            //return observable.Invoke(question);
            return null;
        }

        public Pattern<T, Y, U> Weave<T, Y, U>(object who)
        {
            return null;
        }

        public void Weave<Q, T, Y, U>(object who, Func<Pattern<T, Y, U>, IObservable<Q>> spell)
            where Q : struct, IMatter
            where T : struct, IMatter
            where Y : struct, IMatter
            where U : struct, IMatter
        {
            // where each of generics are Matter
            //  where each of them is wrapped in a Thought that is an IObservable itself

            Type keyT = typeof(Dream<T>);
            var oT = _web[keyT] as Dream<T>;

            Type keyY = typeof(Dream<Y>);
            var oY = _web[keyY] as Dream<Y>;

            Type keyU = typeof(Dream<U>);
            var oU = _web[keyU] as Dream<U>;

            Pattern<T, Y, U> pattern = oT.And(oY).And(oU);

            IObservable<Q> surge = spell.Invoke(pattern);

            Dream<Q> dream = new();
            dream.Initialize(who, surge, oT, oY, oU);

            _web.Add(typeof(Dream<Q>), dream);
        }

        // TODO At some moment Loom objects might be actually returned but for now thats not necessary
        // TODO ! Looms will be usefull as means of informing about existing publishers/providers
        public void Loom<M>(object who, IObservable<M> observable)
            where M : struct, IMatter
        {
            Type key = typeof(Dream<M>);

            observable = observable
                    .Do(onNext: e =>
                    {
                        // TODO circumstances are skipped at the moment
                        Dream<M> thought = _factory.Think(who, e);

                        //TODO add an onNext receiver of debugger
                        // use the above thought
                    });

            Loom<M> loom = new(observable);

            if (_net.ContainsKey(key))
            {
                // TODO allowing multiple producers of matter type
                if (key is TAllowingMultipleSources<M> allowingMultipleSources)
                {
                    _net[key].Add(loom);

                }
                else
                {
                    throw new Exception("Loom of type {key} already exists and it doesnt allow multiple sources");
                }
            }
            else
            {
                _net.Add(key, new List<LoomBase>() { loom });
            }
        }

        public void Loom<Q, A>(object who, Func<Q, IObservable<A>> observable)
            where A : struct, IMatter
            where Q : struct, IMatter
        {
            Type key = typeof(Question<Q, A>);

            if (_net.ContainsKey(key))
            {
                // TODO allowing multiple producers of matter type
            }
            else
            {
                Q trickster = new();

                observable = question => observable
                    .Invoke(question)
                    .Do(onNext: e =>
                    {
                        // TODO circumstances are skipped at the moment
                        Question<Q, A> thought = new();
                        thought.Initialize(who, question, e);

                        //TODO add an onNext receiver of debugger
                        // use the above thought
                    });

                Loom<Q, A> loom = new(observable);

                //_net.Add(key, loom);
            }
        }

        /* ---- ---- 🌠 */
    }

    #endregion // ---------------------------------- Rzeka -------------------------

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 22 July 2022 🌊 */