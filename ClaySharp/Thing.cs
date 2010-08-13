using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CSharp.RuntimeBinder;

namespace ClaySharp {

    public class Thing : IDynamicMetaObjectProvider, IThingBehaviorProvider {
        private readonly ThingBehaviorCollection _behavior;

        public Thing()
            : this(Enumerable.Empty<IThingBehavior>()) {
        }

        public Thing(params IThingBehavior[] behaviors)
            : this(behaviors.AsEnumerable()) {
        }

        public Thing(IEnumerable<IThingBehavior> behaviors) {
            _behavior = new ThingBehaviorCollection(behaviors);
        }
        
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
            return new ThingMetaObject(this, parameter);
        }

        IThingBehavior IThingBehaviorProvider.Behavior {
            get { return _behavior; }
        }
    }
}
