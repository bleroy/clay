using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ClaySharp {
    public interface IClayBehavior {
        object GetMember(Func<object> proceed, object self, string name);
        object SetMember(Func<object> proceed, object self, string name, object value);
        object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args);
        object GetIndex(Func<object> proceed, IEnumerable<object> keys);
        object SetIndex(Func<object> proceed, IEnumerable<object> keys, object value);

        object Convert(Func<object> proceed, object self, Type type, bool isExplicit);
        object BinaryOperation(Func<object> proceed, ExpressionType operation, object value);

        object InvokeMemberMissing(Func<object> proceed, object self, string name, INamedEnumerable<object> args);
        object GetMemberMissing(Func<object> proceed, object self, string name);
        object SetMemberMissing(Func<object> proceed, object self, string name, object value);
    }

    public interface INamedEnumerable<T> : IEnumerable<T> {
        IEnumerable<T> Positional { get; }
        IDictionary<string, T> Named { get; }
    }
}
