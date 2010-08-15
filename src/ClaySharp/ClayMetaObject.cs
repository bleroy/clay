using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace ClaySharp {
    public class ClayMetaObject : DynamicMetaObject {

        public ClayMetaObject(Clay value, Expression expression)
            : base(expression, BindingRestrictions.Empty, value) {
        }

        private Expression GetLimitedSelf() {
            if (Expression.Type == LimitType || Expression.Type.IsEquivalentTo(LimitType)) {
                return Expression;
            }
            return Expression.Convert(Expression, LimitType);
        }

        private Expression GetClayBehavior() {
            return Expression.Property(
                Expression.Convert(Expression, typeof(IClayBehaviorProvider)),
                "Behavior");
        }


        public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
            Trace.WriteLine("BindGetMember");

            var call = Expression.Call(
                GetClayBehavior(),
                typeof(IClayBehavior).GetMethod("GetMember"),
                Expression.Constant(null, typeof(Func<object>)),
                Expression.Constant(binder.Name));

            return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
            Trace.WriteLine("BindSetMember");

            var call = Expression.Call(
                GetClayBehavior(),
                typeof(IClayBehavior).GetMethod("SetMember"),
                Expression.Constant(null, typeof(Func<object>)),
                Expression.Constant(binder.Name),
                Expression.Convert(value.Expression, typeof(object)));

            return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
            Trace.WriteLine("BindInvokeMember");

            var a2 = Expression.NewArrayInit(typeof(object), args.Select(x => Expression.Convert(x.Expression, typeof(Object))));

            var call = Expression.Call(
                GetClayBehavior(),
                typeof(IClayBehavior).GetMethod("InvokeMember"),
                Expression.Constant(null, typeof(Func<object>)),
                GetLimitedSelf(),
                Expression.Constant(binder.Name),
                a2);

            return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType));

        }


        public override DynamicMetaObject BindConvert(ConvertBinder binder) {
            Trace.WriteLine("BindConvert");

            var call = Expression.Call(
                GetClayBehavior(),
                typeof(IClayBehavior).GetMethod("Convert"),
                Expression.Constant(null, typeof(Func<object>)),
                Expression,
                Expression.Constant(binder.Type),
                Expression.Constant(binder.Explicit));

            var convert = Expression.Convert(call, binder.Type);

            return new DynamicMetaObject(convert, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }


        public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
            Trace.WriteLine("BindUnaryOperation");
            throw new NotImplementedException();
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
            Trace.WriteLine("BindGetIndex");

            var a2 = Expression.NewArrayInit(typeof(object), indexes.Select(x => Expression.Convert(x.Expression, typeof(Object))));

            var call = Expression.Call(
                GetClayBehavior(),
                typeof(IClayBehavior).GetMethod("GetIndex"),
                Expression.Constant(null, typeof(Func<object>)),
                a2);

            return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
            Trace.WriteLine("BindSetIndex");

            var a2 = Expression.NewArrayInit(typeof(object), indexes.Select(x => Expression.Convert(x.Expression, typeof(Object))));

            var call = Expression.Call(
                GetClayBehavior(),
                typeof(IClayBehavior).GetMethod("SetIndex"),
                Expression.Constant(null, typeof(Func<object>)),
                a2,
                Expression.Convert(value.Expression, typeof(object)));

            return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes) {
            Trace.WriteLine("BindDeleteIndex");
            throw new NotImplementedException();
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
            Trace.WriteLine("BindInvoke");
            throw new NotImplementedException();
        }

        public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args) {
            Trace.WriteLine("BindCreateInstance");
            throw new NotImplementedException();
        }

        public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder) {
            Trace.WriteLine("BindUnaryOperation");
            throw new NotImplementedException();
        }

        public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg) {
            Trace.WriteLine("BindBinaryOperation");

            var call = Expression.Call(
                GetClayBehavior(),
                typeof(IClayBehavior).GetMethod("BinaryOperation"),
                Expression.Constant(null, typeof(Func<object>)),
                Expression.Constant(binder.Operation),
                Expression.Convert(arg.Expression, typeof(object)));

            return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override IEnumerable<string> GetDynamicMemberNames() {
            Trace.WriteLine("GetDynamicMemberNames");
            throw new NotImplementedException();
        }
    }
}