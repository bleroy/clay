using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClaySharp.Behaviors {
    public class ArrayFactoryBehavior : ClayBehavior {
        public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
            if (name == "Array") {
                dynamic x = new Clay(
                    new InterfaceProxyBehavior(),
                    new PropBehavior(),
                    new ArrayPropAssignmentBehavior(),
                    new ArrayBehavior(),
                    new NilResultBehavior());
                x.AddRange(args);
                return x;
            }
            return proceed();
        }
    }
}
