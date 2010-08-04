using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DynShape.Behaviors {
    public class NilBehavior : ThingBehavior {

        public override object GetMember(Func<object> proceed, string name) {
            return Nil.Instance;
        }

        public override object GetIndex(Func<object> proceed, IEnumerable<object> keys) {
            return Nil.Instance;
        }

        public override object InvokeMember(Func<object> proceed, dynamic self, string name, IEnumerable<object> args) {
            if (args.Any())
                return proceed();

            return Nil.Instance;
        }

        public override object Convert(Func<object> proceed, dynamic self, Type type, bool isExplicit) {
            return null;
        }

        public override object BinaryOperation(Func<object> proceed, ExpressionType operation, object value) {
            switch (operation) {
                case ExpressionType.Equal:
                    return ReferenceEquals(value, Nil.Instance) || value == null;
                case ExpressionType.NotEqual:
                    return !ReferenceEquals(value, Nil.Instance) && value != null;
            }

            return proceed();
        }
    }
}
