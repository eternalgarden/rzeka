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

    public abstract class OperatorObserverBase<TSource, TResult> : IDisposable, IObserver<TSource>
    {
        // oof
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/volatile
        protected internal volatile IObserver<TResult> _observer;
        IDisposable _cancel;

        public OperatorObserverBase(IObserver<TResult> observer, IDisposable cancel)
        {
            _observer = observer;
            _cancel = cancel;
        }

        public abstract void OnNext(TSource value);

        public abstract void OnError(Exception error);

        public abstract void OnCompleted();

        public void Dispose()
        {
            _observer = Rzeka.Utils.EmptyObserver<TResult>.Instance;

            // I shall not preted that I understand threading code
            var target = System.Threading.Interlocked.Exchange(ref _cancel, null);
            
            if (target != null)
            {
                target.Dispose();
            }
        }
    }

    /* ---- ---- ⛺ */
}
/* maria aurelia at 25 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */