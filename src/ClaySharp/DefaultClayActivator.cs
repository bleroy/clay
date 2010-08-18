using System;
using System.Collections.Generic;
using System.Dynamic;

namespace ClaySharp {
    public class DefaultClayActivator : IClayActivator {
        public dynamic CreateInstance<TBase>(IEnumerable<IClayBehavior> behaviors)
        {
            var baseType = typeof(TBase);
            var isDynamicMetaObjectProvider = typeof(IDynamicMetaObjectProvider).IsAssignableFrom(baseType);
            var isClayBehaviorProvider = typeof(IClayBehaviorProvider).IsAssignableFrom(baseType);
            
            var constructorArguments = new object[]{behaviors};
            return Activator.CreateInstance(baseType, constructorArguments);
        }
    }
}
