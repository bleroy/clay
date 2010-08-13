using System;
using System.Collections.Generic;
using System.Linq;

namespace ClaySharp.Behaviors {
    public class PropBehavior : ThingBehavior {
        readonly Dictionary<object, object> _props = new Dictionary<object, object>();


        public override object GetMember(Func<object> proceed, string name) {
            object value;
            return _props.TryGetValue(name, out value) ? value : null;
        }

        public override object SetMember(Func<object> proceed, string name, object value) {
            return _props[name] = value;
        }

        public override object InvokeMember(Func<object> proceed, dynamic self, string name, IEnumerable<object> args) {
            if (!args.Any()) {
                return GetMember(proceed, name);
            }

            if (args.Count() == 1) {
                SetMember(proceed, name, args.Single());
                return self;
            }

            return proceed();
        }

        public override object GetIndex(Func<object> proceed, IEnumerable<object> keys) {
            if (keys.Count() != 1) proceed();

            object value;
            return _props.TryGetValue(keys.Single(), out value) ? value : null;
        }

        public override object SetIndex(Func<object> proceed, IEnumerable<object> keys, object value) {
            if (keys.Count() != 1) proceed();

            return _props[keys.Single()] = value;
        }
    }
}
