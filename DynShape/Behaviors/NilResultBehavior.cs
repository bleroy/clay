using System;
using System.Collections.Generic;
using System.Linq;

namespace DynShape.Behaviors {
    public class NilResultBehavior : ThingBehavior {

        public override object GetMember(Func<object> proceed, string name) {
            return proceed() ?? Nil.Instance;
        }

        public override object GetIndex(Func<object> proceed, IEnumerable<object> keys) {
            return proceed() ?? Nil.Instance;
        }

        public override object InvokeMember(Func<object> proceed, dynamic self, string name, IEnumerable<object> args) {
            if (args.Any())
                return proceed();

            return proceed() ?? Nil.Instance;
        }
    }
}