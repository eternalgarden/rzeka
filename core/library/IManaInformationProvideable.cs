using System;

namespace Rzeka;
public interface IManaInformationProvideable
{
    Type LastChangedType { get; }
    bool IsManaOfTypeAvailable<T>() where T : IMatter;
    bool IsManaOfTypeAvailable(Type type);
}
