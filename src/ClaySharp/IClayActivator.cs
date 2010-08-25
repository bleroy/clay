using System.Collections.Generic;

namespace ClaySharp
{
    public interface IClayActivator {
        dynamic CreateInstance<TBase>(IEnumerable<IClayBehavior> behaviors, IEnumerable<object> arguments);
    }
}