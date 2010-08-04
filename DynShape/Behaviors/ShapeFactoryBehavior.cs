using System;
using System.Collections.Generic;
using System.Linq;

namespace DynShape.Behaviors {
    public class ShapeFactoryBehavior : ThingBehavior {
        public override object InvokeMember(Func<object> proceed, dynamic self, string name, IEnumerable<object> args) {

            dynamic shape = new Thing(new PropBehavior(), new NilResultBehavior(), new InterfaceProxyBehavior());

            shape.ShapeName = name;

            if (args.Count() == 1) {
                var options = args.Single();
                foreach (var optionsProperty in options.GetType().GetProperties()) {
                    shape[optionsProperty.Name] = optionsProperty.GetValue(options, null);
                }
            }

            return shape;
        }
    }
}
