using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ClaySharp.Behaviors {
    public class NilBehavior : ClayBehavior {

        public override object GetMember(Func<object> proceed, object self, string name) {
            return Nil.Instance;
        }

        public override object GetIndex(Func<object> proceed, object self, IEnumerable<object> keys) {
            return Nil.Instance;
        }

        public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
            if (args.Any())
                return proceed();

            if (name == "ToString") {
                return string.Empty;
            }

            return Nil.Instance;
        }

        public override object Convert(Func<object> proceed, object self, Type type, bool isExplicit) {
            if (type.IsInterface)
                return proceed();

            return null;
        }

        public override object BinaryOperation(Func<object> proceed, object self, ExpressionType operation, object value) {
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
