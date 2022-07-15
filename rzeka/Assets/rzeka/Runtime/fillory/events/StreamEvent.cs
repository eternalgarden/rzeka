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

namespace RzekaRiver
{
    /* ---- ---- ⛺ */

    // * In future maybe when unity fully implements default interfaces return here
    // * They are still not flattened
    // * Amd you need to cast to strand before you use it.
    public interface Strand
    {
        public static void Pluck<T>(T thought) where T : StreamEvent
        {
            Rzeka.Pluck(thought);
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

    public sealed class UnknownStrand : StreamEvent
    {
        public override string Description => "who could it be?";
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
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 12 June 2022 🌊 */