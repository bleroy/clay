using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClaySharp.Behaviors {
    public class ArrayPropAssignmentBehavior : ClayBehavior {

        public override object InvokeMember(Func<object> proceed, dynamic self, string name, IEnumerable<object> args) {
            return
                IfSingleArray(args, arr => { self[name] = arr; return self; }, () =>
                    IfTwoOrMoreArgs(args, arr => { self[name] = arr; return self; },
                        proceed));
        }

        private object IfTwoOrMoreArgs(IEnumerable<object> args, Func<dynamic, object> func, Func<object> proceed) {
            if (args.Count() < 2)
                return proceed();

            return func(NewArray().AddRange(args));
        }

        private object IfSingleArray(IEnumerable<object> args, Func<dynamic, object> func, Func<object> proceed) {
            if (args.Count() != 1)
                return proceed();

            var arr = args.Single();
            if (arr == null)
                return proceed();

            if (!typeof(Array).IsAssignableFrom(arr.GetType()))
                return proceed();

            return func(NewArray().AddRange(arr));
        }

        private static dynamic NewArray() {
            return new Clay(
                new InterfaceProxyBehavior(),
                new PropBehavior(),
                new ArrayPropAssignmentBehavior(),
                new ArrayBehavior(), 
                new NilResultBehavior());
        }
    }
}
