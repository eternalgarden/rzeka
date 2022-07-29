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
    /* ---- ---- ⛺ */

    public abstract class ThoughtBase : IDisposable
    {
        protected ThoughtBase[] _circumstances;
        protected object _who;
        protected bool _wasDisposed;
        protected object _thoughtLock = new();

        //
        // ⛺ ─── Properties ───────────────────────────────────────────────────
        //
        #region Properties

        public abstract string Description { get; }

        public object Context
        {
            get
            {
                if (_who is null) throw new Exception("<context> was not initialized");

                return _who;
            }
        }

        public ThoughtBase[] Circumstances
        {
            get
            {
                if (_circumstances is null) throw new Exception("<circumnstances> were not initialized");

                return _circumstances;
            }
        }

        #endregion // ---------------------------------- Properties -------------------------

        public virtual void Dispose()
        {
            lock (_thoughtLock)
            {
                _wasDisposed = true;
                _circumstances = null;
                _who = null;
            }
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 12 June 2022 🌊 */