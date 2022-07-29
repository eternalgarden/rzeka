/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;

namespace Rzeka
{
    public abstract class Thought<T,U> : ThoughtBase
        where T : Matter
        where U : Matter
    {
        readonly T inputMatter;

        public Thought(T inputMatter)
        {
            this.inputMatter = inputMatter;
        }
    }

    public abstract class Thought<T> : ThoughtBase where T : Matter
    {
        [Obsolete] T _matter;
        [Obsolete] bool _wasInitialized;

        [Obsolete] public T Matter
        {
            get
            {
                if (_matter is null) throw new Exception("<gift> was not initialized");

                return _matter;
            }
        }

        [Obsolete] public virtual void Initialize(T matter, object context, params ThoughtBase[] circumstances)
        {
            lock (_thoughtLock)
            {
                if (_wasDisposed is false)
                {
                    _matter = matter ?? throw new ArgumentNullException(nameof(matter));
                    _who = context ?? throw new ArgumentNullException(nameof(context));
                    _circumstances = circumstances ?? throw new ArgumentException("There has to be at least single cause of an event. If it is a root event please use Causes.Root");
                    _wasInitialized = true;
                }
                else if (_wasInitialized is true)
                {
                    throw new Exception("Thought was already initialized.");
                }
                else
                {
                    throw new Exception("Tried to Initialize a thought that has already been disposed");
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            _matter = null;
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 12 June 2022 🌊 */