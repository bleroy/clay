using System;
using System.Collections.Generic;

namespace ClaySharp {
    public static class ClayActivator {
        static ClayActivator() {
            var instance = new DefaultClayActivator();
            ServiceLocator = () => instance;
        }

        public static Func<IClayActivator> ServiceLocator { get; set; }

        public static dynamic CreateInstance<TBase>(IEnumerable<IClayBehavior> behaviors, params object[] arguments) {
            return ServiceLocator().CreateInstance<TBase>(behaviors, arguments);
        }

        public static dynamic CreateInstance(IEnumerable<IClayBehavior> behaviors, params object[] arguments) {
            return CreateInstance<Clay>(behaviors, arguments);
        }
    }
}
