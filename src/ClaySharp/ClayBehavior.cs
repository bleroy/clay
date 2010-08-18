using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ClaySharp {
    public abstract class ClayBehavior : IClayBehavior {
        public virtual object GetMember(Func<object> proceed, string name) { return proceed(); }
        public virtual object SetMember(Func<object> proceed, string name, object value) { return proceed(); }
        public virtual object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args) { return proceed(); }
        public virtual object GetIndex(Func<object> proceed, IEnumerable<object> keys) { return proceed(); }
        public virtual object SetIndex(Func<object> proceed, IEnumerable<object> keys, object value) { return proceed(); }
        public virtual object Convert(Func<object> proceed, object self, Type type, bool isExplicit) { return proceed(); }
        public virtual object BinaryOperation(Func<object> proceed, ExpressionType operation, object value) { return proceed(); }
    }
}
