/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx

** original comment by neuecc:

"this code is borrowed from RxOfficial(rx.codeplex.com) and modified"

"Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information."

** end of comment

Hmmmm, the license is just the MIT one in neuecc repository

*/

using System;

namespace Rzeka
{
    /// <summary>
    /// Abstract base class for scheduled work items.
    /// </summary>
    internal class ScheduledItem : IComparable<ScheduledItem>
    {
        /* 🌊 ---- ---- */
    
        private readonly DisposableBoolean _disposable = new DisposableBoolean();
        private readonly TimeSpan _dueTime;
        private readonly Action _action;

        /// <summary>
        /// Creates a new scheduled work item to run at the specified time.
        /// </summary>
        /// <param name="dueTime">Absolute time at which the work item has to be executed.</param>
        public ScheduledItem(Action action, TimeSpan dueTime)
        {
            _dueTime = dueTime;
            _action = action;
        }

        /// <summary>
        /// Gets the absolute time at which the item is due for invocation.
        /// </summary>
        public TimeSpan DueTime
        {
            get { return _dueTime; }
        }

        /// <summary>
        /// Invokes the work item.
        /// </summary>
        public void Invoke()
        {
            if (!_disposable.IsDisposed)
            {
                _action();
            }
        }

        #region Inequality

        /// <summary>
        /// Compares the work item with another work item based on absolute time values.
        /// </summary>
        /// <param name="other">Work item to compare the current work item to.</param>
        /// <returns>Relative ordering between this and the specified work item.</returns>
        /// <remarks>The inequality operators are overloaded to provide results consistent with the IComparable implementation. Equality operators implement traditional reference equality semantics.</remarks>
        public int CompareTo(ScheduledItem other)
        {
            // MSDN: By definition, any object compares greater than null, and two null references compare equal to each other. 
            if (object.ReferenceEquals(other, null))
                return 1;

            return DueTime.CompareTo(other.DueTime);
        }

        /// <summary>
        /// Determines whether one specified ScheduledItem&lt;TAbsolute&gt; object is due before a second specified ScheduledItem&lt;TAbsolute&gt; object.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if the DueTime value of left is earlier than the DueTime value of right; otherwise, false.</returns>
        /// <remarks>This operator provides results consistent with the IComparable implementation.</remarks>
        public static bool operator <(ScheduledItem left, ScheduledItem right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Determines whether one specified ScheduledItem&lt;TAbsolute&gt; object is due before or at the same of a second specified ScheduledItem&lt;TAbsolute&gt; object.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if the DueTime value of left is earlier than or simultaneous with the DueTime value of right; otherwise, false.</returns>
        /// <remarks>This operator provides results consistent with the IComparable implementation.</remarks>
        public static bool operator <=(ScheduledItem left, ScheduledItem right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Determines whether one specified ScheduledItem&lt;TAbsolute&gt; object is due after a second specified ScheduledItem&lt;TAbsolute&gt; object.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if the DueTime value of left is later than the DueTime value of right; otherwise, false.</returns>
        /// <remarks>This operator provides results consistent with the IComparable implementation.</remarks>
        public static bool operator >(ScheduledItem left, ScheduledItem right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Determines whether one specified ScheduledItem&lt;TAbsolute&gt; object is due after or at the same time of a second specified ScheduledItem&lt;TAbsolute&gt; object.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if the DueTime value of left is later than or simultaneous with the DueTime value of right; otherwise, false.</returns>
        /// <remarks>This operator provides results consistent with the IComparable implementation.</remarks>
        public static bool operator >=(ScheduledItem left, ScheduledItem right)
        {
            return left.CompareTo(right) >= 0;
        }

        #endregion

        #region Equality

        /// <summary>
        /// Determines whether two specified ScheduledItem&lt;TAbsolute, TValue&gt; objects are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if both ScheduledItem&lt;TAbsolute, TValue&gt; are equal; otherwise, false.</returns>
        /// <remarks>This operator does not provide results consistent with the IComparable implementation. Instead, it implements reference equality.</remarks>
        public static bool operator ==(ScheduledItem left, ScheduledItem right)
        {
            return object.ReferenceEquals(left, right);
        }

        /// <summary>
        /// Determines whether two specified ScheduledItem&lt;TAbsolute, TValue&gt; objects are inequal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if both ScheduledItem&lt;TAbsolute, TValue&gt; are inequal; otherwise, false.</returns>
        /// <remarks>This operator does not provide results consistent with the IComparable implementation. Instead, it implements reference equality.</remarks>
        public static bool operator !=(ScheduledItem left, ScheduledItem right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether a ScheduledItem&lt;TAbsolute&gt; object is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare to the current ScheduledItem&lt;TAbsolute&gt; object.</param>
        /// <returns>true if the obj parameter is a ScheduledItem&lt;TAbsolute&gt; object and is equal to the current ScheduledItem&lt;TAbsolute&gt; object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return object.ReferenceEquals(this, obj);
        }

        /// <summary>
        /// Returns the hash code for the current ScheduledItem&lt;TAbsolute&gt; object.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        public IDisposable Cancellation
        {
            get
            {
                return _disposable;
            }
        }

        /// <summary>
        /// Gets whether the work item has received a cancellation request.
        /// </summary>
        public bool IsCanceled
        {
            get { return _disposable.IsDisposed; }
        }
    }
}
/* maria aurelia at 24 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */