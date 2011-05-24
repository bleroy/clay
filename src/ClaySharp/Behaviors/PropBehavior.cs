using System;
using System.Collections.Generic;
using System.Linq;

namespace ClaySharp.Behaviors {
    public class PropBehavior : ClayBehavior {
        readonly Dictionary<object, object> _props = new Dictionary<object, object>();

        public override object GetMembers(System.Func<object> proceed, object self, IDictionary<string, object> members) {
            foreach (var pair in _props) {
                members.Add(pair.Key.ToString(), pair.Value);
            }

            return proceed();
        }

        public override object GetMemberMissing(Func<object> proceed, object self, string name) {
            object value;
            return _props.TryGetValue(name, out value) ? value : null;
        }

        public override object SetMemberMissing(Func<object> proceed, object self, string name, object value) {
            return _props[name] = value;
        }

        public override object InvokeMemberMissing(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
            if (!args.Any()) {
                return GetMemberMissing(proceed, self, name);
            }

            if (args.Count() == 1) {
                SetMemberMissing(proceed, self, name, args.Single());
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
