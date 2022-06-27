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
    /* 🌊 ---- ---- */

    /* original neuecc comment:

    should be use Interlocked.CompareExchange for Threadsafe?
    but CompareExchange cause ExecutionEngineException on iOS.
    AOT...
    use lock instead

    */

    /// <remarks>
    /// Still not fully certain what is the point of such fuss around complexity of this class.
    /// Lets you overwrite the underlying Disposable.
    /// </remarks>
    public sealed class SingleAssignmentDisposable : IDisposable, ICancelable
    {
        readonly object gate = new object();
        IDisposable current;
        bool disposed;

        public bool IsDisposed { get { lock (gate) { return disposed; } } }

        /*

        TODO QUESTION

        So I assume there is a reason why we cannot simply use a readonly IDisposable
        variable exposed through a public getter.

        Is the only reason for that the mysterious call to value.Dispose()?

        */
        public IDisposable Disposable
        {
            get
            {
                return current;
            }
            set
            {
                var old = default(IDisposable);
                bool alreadyDisposed;

                lock (gate)
                {
                    alreadyDisposed = disposed;

                    // If current is anything else than null, an error below will be thrown.
                    old = current;

                    if (!alreadyDisposed)
                    {
                        if (value == null) return;
                        current = value;
                    }
                }

                // * Interesting
                // In case when it's old value was already disposed before.
                // Dispose the value you try to put in aswell.
                // ? Whats the use of that?
                if (alreadyDisposed && value != null)
                {
                    value.Dispose();
                    return;
                }

                // * Mmmmm!
                // ? Oki, so is it all like a kindof convoluted way to have an immutable 
                // ? reference to a Disposable object?
                if (old != null) throw new InvalidOperationException("Disposable is already set");
            }
        }

        public void Dispose()
        {
            // ? Why not
            //  = default(IDisposable);
            // ? like above?
            // I am to scared to touch anything in that code at the moment.
            // With all that threaded code around I feel like touching some black magic here
            // as if in a Roadside Picnic zona.
            IDisposable old = null;

            lock (gate)
            {
                if (!disposed)
                {
                    //* isn't this a perfect example of how shady working with automatic garbage collecting is sometimes
                    //* like you can't just say dispose on the current object, dispose doesn't really mean much in c#
                    //* it's only like a pattern to follow
                    //* so you make a thing like this to be dead-sure there are no references hanging around
                    disposed = true;
                    old = current;
                    current = null;
                }
            }

            if (old != null) old.Dispose();
        }
    }

    /* ---- ---- ⛺ */
}
/* maria aurelia at 25 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */