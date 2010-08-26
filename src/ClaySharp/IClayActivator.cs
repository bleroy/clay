using System;
using System.Collections.Generic;

namespace ClaySharp
{
    public interface IClayActivator {
        dynamic CreateInstance(Type baseType, IEnumerable<IClayBehavior> behaviors, IEnumerable<object> arguments);
    }
}