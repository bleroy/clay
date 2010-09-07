using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using ClaySharp.Implementation;

namespace ClaySharp {

    public class Clay : IDynamicMetaObjectProvider, IClayBehaviorProvider {
        private readonly ClayBehaviorCollection _behavior;

        public Clay()
            : this(Enumerable.Empty<IClayBehavior>()) {
        }

        public Clay(params IClayBehavior[] behaviors)
            : this(behaviors.AsEnumerable()) {
        }

        public Clay(IEnumerable<IClayBehavior> behaviors) {
            _behavior = new ClayBehaviorCollection(behaviors);
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
            return new ClayMetaObject(this, parameter);
        }

        IClayBehavior IClayBehaviorProvider.Behavior {
            get { return _behavior; }
        }

        public override string ToString() {
            var fallback = base.ToString();
            return _behavior.InvokeMember(() => fallback, this, "ToString", Arguments.Empty()) as string;
        }

    }
}
