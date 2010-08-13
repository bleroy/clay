using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ClaySharp {
    public interface IThingBehavior {
        object GetMember(Func<object> proceed, string name);
        object SetMember(Func<object> proceed, string name, object value);
        object InvokeMember(Func<object> proceed, dynamic self, string name, IEnumerable<object> args);
        object GetIndex(Func<object> proceed, IEnumerable<object> keys);
        object SetIndex(Func<object> proceed, IEnumerable<object> keys, object value);

        object Convert(Func<object> proceed, dynamic self, Type type, bool isExplicit);
        object BinaryOperation(Func<object> proceed, ExpressionType operation, object value);
    }
}
