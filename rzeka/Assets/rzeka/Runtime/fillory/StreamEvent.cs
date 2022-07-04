/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Linq;

namespace Rzeka.Stream
{
    /*

    Each event also describes the type of strand it creates

    - Hot (Subject)
        - no default value
        - throws error when there are no listeners to a published event?
    - Cold (ReplaySubject)
        - no default value
        - published events are getting stored even if there are no listeners at the time
        - defines it's buffer size
    - Behaved (BehaviourSubject)
        - has default value

    */
    [Flags]
    public enum StrandDescriptors
    {
        Hot,
        Replay,
        Behaviour,
        Pooled
    }

    public class StrandAttribute : Attribute
    {
        public StrandAttribute() { }
    }

    public class HotAttribute : StrandAttribute
    {
        public HotAttribute() { }
    }

    public class HoardingAttribute : StrandAttribute
    {
        private readonly object defaultValue;
        private readonly int buffer;
        private readonly bool pooled;
        private readonly int poolSize;

        public HoardingAttribute(object defaultValue, int buffer)
        {
            this.defaultValue = defaultValue;
            this.buffer = buffer;
        }

        public HoardingAttribute(object defaultValue, int buffer, bool pooled, int poolSize)
            : this(defaultValue, buffer)
        {
            this.defaultValue = defaultValue;
            this.pooled = pooled;
            this.poolSize = poolSize;
        }
    }

    public class KindAttribute : StrandAttribute
    {
        private object defaultValue;

        public KindAttribute(object defaultValue, int buffer)
        {
            this.defaultValue = defaultValue;
        }
    }

    //    [Hot] 
    public abstract class StreamEvent
    {
        object context;
        StreamEvent[] circumstances;
        public abstract string Description { get; }

        //
        // ⛺ ─── Properties ───────────────────────────────────────────────────
        //
        #region Properties

        public virtual StreamEvent Default => null;

        public object Context
        {
            get
            {
                if (context is null) throw new Exception("<context> was not initialized");

                return context;
            }
        }

        public StreamEvent[] Circumstances
        {
            get
            {
                if (circumstances is null) throw new Exception("<circumnstances> were not initialized");

                return circumstances;
            }
        }

        public bool this[StreamEvent other]
        {
            // * optimize
            get => circumstances.Contains(other);
        }

        #endregion // ---------------------------------- Properties -------------------------

        public void Initialize(object context, params StreamEvent[] circumstances)
        {
            if (circumstances is null)
            {
                throw new ArgumentException("There has to be at least single cause of an event. If it is a root event please use Causes.Root");
            }

            this.context = context;
            this.circumstances = circumstances;
        }
    }

    public interface IResettable
    {
        void Reset();
    }

    public sealed class RootEvent : StreamEvent
    {
        // new private object Context;
        // new private StreamEvent[] Circumstances;
        // new private bool this[StreamEvent other]
        // {
        //     get => false;
        // }
        public override string Description => throw new NotImplementedException();
    }

    public abstract class Gift { }

    public class SimpleGift<T> : Gift
    {
        readonly T content;

        public T Content => content;

        public SimpleGift(T content)
        {
            this.content = content;
        }
    }

    public abstract class StreamGiftEvent<T> : StreamEvent where T : Gift
    {
        T gift;

        public T Gift
        {
            get
            {
                if (gift is null) throw new Exception("<gift> was not initialized");

                return gift;
            }
        }

        public void Initialize(T gift, object context, params StreamEvent[] circumstances)
        {
            this.gift = gift;
            base.Initialize(context, circumstances);
        }

        new private void Initialize(object context, params StreamEvent[] circumstances) { }


        public virtual bool HasDefault(out T defaultValue)
        {
            defaultValue = null;
            return false;
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 12 June 2022 🌊 */