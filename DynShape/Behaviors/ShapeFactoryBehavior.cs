using System;
using System.Collections.Generic;
using System.Linq;

namespace DynShape.Behaviors {
    public class ShapeFactoryBehavior : ThingBehavior {
        public override object InvokeMember(Func<object> proceed, dynamic self, string name, IEnumerable<object> args) {

            var shape = new Thing(new PropBehavior(), new NilResultBehavior());

            shape.Behaviors.SetMember(null, "ShapeName", name);

            if (args.Count() == 1) {
                var options = args.Single();
                foreach (var optionsProperty in options.GetType().GetProperties()) {
                    var option = optionsProperty.GetValue(options, null);
                    shape.Behaviors.SetMember(null, optionsProperty.Name, option);
                }
            }

            return shape;
        }
    }
}
