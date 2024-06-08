using System;

namespace Rzeka
{
    public interface IManaInformationProvideable
    {
        Type LastChangedType { get; }
        bool IsManaOfTypeAvailable<T>() where T : TMatter;
        bool IsManaOfTypeAvailable(Type type);
    }
}