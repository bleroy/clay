using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CSharp.RuntimeBinder;

namespace DynShape {

    public class Thing : IDynamicMetaObjectProvider {
        public Thing()
            : this(Enumerable.Empty<IThingBehavior>()) {
        }

        public Thing(params IThingBehavior[] behaviors)
            : this(behaviors.AsEnumerable()) {
        }

        public Thing(IEnumerable<IThingBehavior> behaviors) {
            Behaviors = new ThingBehaviorCollection(behaviors);
        }

        public ThingBehaviorCollection Behaviors { get; set; }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
            return new ThingMetaObject(this, parameter);
        }
    }
}
