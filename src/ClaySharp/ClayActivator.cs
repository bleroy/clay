using System;
using System.Collections.Generic;

namespace ClaySharp {
    public static class ClayActivator {
        static ClayActivator() {
            var instance = new DefaultClayActivator();
            ServiceLocator = () => instance;
        }

        public static Func<IClayActivator> ServiceLocator { get; set; }

        public static dynamic CreateInstance<TBase>(IEnumerable<IClayBehavior> behaviors) {
            return ServiceLocator().CreateInstance<TBase>(behaviors);
        }

        public static dynamic CreateInstance(IEnumerable<IClayBehavior> behaviors) {
            return CreateInstance<Clay>(behaviors);
        }
    }
}
