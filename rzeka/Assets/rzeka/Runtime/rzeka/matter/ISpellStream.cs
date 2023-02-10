using System;

namespace Rzeka
{
    public interface ISpellStream : IDisposable
    {
        Type MatterType { get; }
        bool IsAvailable { get; }
    }
}