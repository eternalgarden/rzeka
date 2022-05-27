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
    public interface IScheduler
    {
        DateTimeOffset Now { get; }

        // Interface is changed from official Rx for avoid iOS AOT problem 
        // (state is dangerous).

        IDisposable Schedule(Action action);

        IDisposable Schedule(TimeSpan dueTime, Action action);
    }

    public interface ISchedulerPeriodic
    {
        IDisposable SchedulePeriodic(TimeSpan period, Action action);
    }

    public interface ISchedulerLongRunning
    {
        IDisposable ScheduleLongRunning(Action<ICancelable> action);
    }

    public interface ISchedulerQueueing
    {
        void ScheduleQueueing<T>(ICancelable cancel, T state, Action<T> action);
    }
}
/* maria aurelia at 24 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */