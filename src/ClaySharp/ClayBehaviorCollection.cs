using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ClaySharp {
    public class ClayBehaviorCollection : List<IClayBehavior>, IClayBehavior {
        public ClayBehaviorCollection(IEnumerable<IClayBehavior> behaviors)
            : base(behaviors) {
        }

        object Execute(Func<object> proceed, Func<Func<object>, IClayBehavior, Func<object>> linker) {
            return this.Aggregate(proceed, linker)();
        }

        public object GetMember(Func<object> proceed, string name) {
            return Execute(proceed, (next, behavior) => () => behavior.GetMember(next, name));
        }

        public object SetMember(Func<object> proceed, string name, object value) {
            return Execute(proceed, (next, behavior) => () => behavior.SetMember(next, name, value));
        }

        public object InvokeMember(Func<object> proceed, dynamic self, string name, IEnumerable<object> args) {
            return Execute(proceed, (next, behavior) => () => behavior.InvokeMember(next, self, name, args));
        }

        public object GetIndex(Func<object> proceed, IEnumerable<object> keys) {
            return Execute(proceed, (next, behavior) => () => behavior.GetIndex(next, keys));
        }

        public object SetIndex(Func<object> proceed, IEnumerable<object> keys, object value) {
            return Execute(proceed, (next, behavior) => () => behavior.SetIndex(next, keys, value));
        }

        public object Convert(Func<object> proceed, dynamic self, Type type, bool isExplicit) {
            return Execute(proceed, (next, behavior) => () => behavior.Convert(next, self, type, isExplicit));
        }

        public object BinaryOperation(Func<object> proceed, ExpressionType operation, object value) {
            return Execute(proceed, (next, behavior) => () => behavior.BinaryOperation(next, operation, value));
        }
    }
}
