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
    public abstract class StreamEvent
    {
        object context;
        StreamEvent[] circumstances;

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

    public sealed class RootEvent : StreamEvent
    {
        new private object Context;
        new private StreamEvent[] Circumstances;
        new private bool this[StreamEvent other]
        {
            get => false;
        }
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

        public abstract string Description { get; }

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