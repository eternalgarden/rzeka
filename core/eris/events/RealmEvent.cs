using System;

namespace Rzeka
{
    [Serializable]
    public abstract class RealmEvent : IDisposable
    {
        public readonly Guid Guid;
        public readonly DateTimeOffset Timestamp;

        protected RealmEvent()
        {
            Guid = Guid.NewGuid();
            Timestamp = DateTimeOffset.UtcNow;
        }

        public void Dispose()
        {
        }
    }
}