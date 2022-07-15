/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;

namespace RzekaRiver
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

    public interface HasDefaultValue<T>
    {
        // * rework into traits with default implementation once unity catches up
        T DefaultValue();
    }

    public interface HasMemory
    {
        // * rework into traits with default implementation once unity catches up
        int MemoryCapacity { get; }
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

        public KindAttribute(object defaultValue)
        {
            this.defaultValue = defaultValue;
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 12 June 2022 🌊 */