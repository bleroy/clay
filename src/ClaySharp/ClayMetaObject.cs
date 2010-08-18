using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using ClaySharp.Implementation;

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
            
            var binderFallback = binder.FallbackGetMember(this);

            var call = Expression.Call(
                GetClayBehavior(),
                typeof(IClayBehavior).GetMethod("GetMember"),
                Expression.Lambda(binderFallback.Expression),
                Expression.Constant(binder.Name));

            return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderFallback.Restrictions));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
            Trace.WriteLine("BindSetMember");

            var binderFallback = binder.FallbackSetMember(this, value);

            var call = Expression.Call(
                GetClayBehavior(),
                typeof(IClayBehavior).GetMethod("SetMember"),
                Expression.Lambda(binderFallback.Expression),
                Expression.Constant(binder.Name),
                Expression.Convert(value.Expression, typeof(object)));

            return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderFallback.Restrictions));
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
            Trace.WriteLine("BindInvokeMember");
            


            var argValues = Expression.NewArrayInit(typeof(object), args.Select(x => Expression.Convert(x.Expression, typeof(Object))));
            var argNames = Expression.Constant(binder.CallInfo.ArgumentNames, typeof(IEnumerable<string>));
            var argNamedEnumerable = Expression.Call(typeof(Arguments).GetMethod("From"), argValues, argNames);

            var binderFallback = binder.FallbackInvokeMember(this, args);

            var call = Expression.Call(
                GetClayBehavior(),
                typeof(IClayBehavior).GetMethod("InvokeMember"),
                Expression.Lambda(binderFallback.Expression),
                GetLimitedSelf(),
                Expression.Constant(binder.Name),
                argNamedEnumerable);

            return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderFallback.Restrictions));

        }


        public override DynamicMetaObject BindConvert(ConvertBinder binder) {
            Trace.WriteLine("BindConvert");

            var binderFallback = binder.FallbackConvert(this);

            //TODO: all proceed Lambda expressions will likely need an object typecast... utility method...

            var call = Expression.Call(
                GetClayBehavior(),
                typeof(IClayBehavior).GetMethod("Convert"),
                Expression.Lambda(Expression.Convert(binderFallback.Expression, typeof(Object))),
                Expression,
                Expression.Constant(binder.Type),
                Expression.Constant(binder.Explicit));

            var convert = Expression.Convert(call, binder.Type);

            return new DynamicMetaObject(convert, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderFallback.Restrictions));
        }


        public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
            Trace.WriteLine("BindUnaryOperation");
            throw new NotImplementedException();
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
            Trace.WriteLine("BindGetIndex");

            var a2 = Expression.NewArrayInit(typeof(object), indexes.Select(x => Expression.Convert(x.Expression, typeof(Object))));

            var binderFallback = binder.FallbackGetIndex(this, indexes);

            var call = Expression.Call(
                GetClayBehavior(),
                typeof(IClayBehavior).GetMethod("GetIndex"),
                Expression.Lambda(binderFallback.Expression),
                a2);

            return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderFallback.Restrictions));
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
            Trace.WriteLine("BindSetIndex");

            var a2 = Expression.NewArrayInit(typeof(object), indexes.Select(x => Expression.Convert(x.Expression, typeof(Object))));

            var binderFallback = binder.FallbackSetIndex(this, indexes, value);

            var call = Expression.Call(
                GetClayBehavior(),
                typeof(IClayBehavior).GetMethod("SetIndex"),
                Expression.Lambda(binderFallback.Expression),
                a2,
                Expression.Convert(value.Expression, typeof(object)));

            return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderFallback.Restrictions));
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