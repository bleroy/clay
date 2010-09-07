using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ClaySharp.Implementation;

namespace ClaySharp {
    public class ClayMetaObject : DynamicMetaObject {
        // ReSharper disable InconsistentNaming
        private static readonly MethodInfo IClayBehavior_InvokeMember = typeof(IClayBehavior).GetMethod("InvokeMember");
        private static readonly MethodInfo IClayBehavior_GetMember = typeof(IClayBehavior).GetMethod("GetMember");
        private static readonly MethodInfo IClayBehavior_SetMember = typeof(IClayBehavior).GetMethod("SetMember");
        private static readonly MethodInfo IClayBehavior_GetIndex = typeof(IClayBehavior).GetMethod("GetIndex");
        private static readonly MethodInfo IClayBehavior_SetIndex = typeof(IClayBehavior).GetMethod("SetIndex");
        private static readonly MethodInfo IClayBehavior_BinaryOperation = typeof(IClayBehavior).GetMethod("BinaryOperation");
        private static readonly MethodInfo IClayBehavior_Convert = typeof(IClayBehavior).GetMethod("Convert");

        private static readonly MethodInfo IClayBehavior_InvokeMemberMissing = typeof(IClayBehavior).GetMethod("InvokeMemberMissing");
        private static readonly MethodInfo IClayBehavior_GetMemberMissing = typeof(IClayBehavior).GetMethod("GetMemberMissing");
        private static readonly MethodInfo IClayBehavior_SetMemberMissing = typeof(IClayBehavior).GetMethod("SetMemberMissing");
        private static readonly MethodInfo IClayBehavior_ConvertMissing = typeof(IClayBehavior).GetMethod("ConvertMissing");

        // ReSharper restore InconsistentNaming

        public ClayMetaObject(object value, Expression expression)
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

            var binderDefault = binder.FallbackGetMember(this);


            var missingLambda = Expression.Lambda(Expression.Call(
                GetClayBehavior(),
                IClayBehavior_GetMemberMissing,
                Expression.Lambda(binderDefault.Expression),
                GetLimitedSelf(),
                Expression.Constant(binder.Name)));

            var call = Expression.Call(
                GetClayBehavior(),
                IClayBehavior_GetMember,
                missingLambda,
                GetLimitedSelf(),
                Expression.Constant(binder.Name));
            
            var dynamicSuggestion = new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderDefault.Restrictions));

            return binder.FallbackGetMember(this, dynamicSuggestion);
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
            Trace.WriteLine("BindSetMember");

            var binderDefault = binder.FallbackSetMember(this, value);

            var missingLambda = Expression.Lambda(Expression.Call(
                GetClayBehavior(),
                IClayBehavior_SetMemberMissing,
                Expression.Lambda(binderDefault.Expression),
                GetLimitedSelf(),
                Expression.Constant(binder.Name),
                Expression.Convert(value.Expression, typeof(object))));

            var call = Expression.Call(
                GetClayBehavior(),
                IClayBehavior_SetMember,
                missingLambda,
                GetLimitedSelf(),
                Expression.Constant(binder.Name),
                Expression.Convert(value.Expression, typeof(object)));

            var dynamicSuggestion = new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderDefault.Restrictions));

            return binder.FallbackSetMember(this, value, dynamicSuggestion);
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
            Trace.WriteLine("BindInvokeMember");

            var argValues = Expression.NewArrayInit(typeof(object), args.Select(x => Expression.Convert(x.Expression, typeof(Object))));
            var argNames = Expression.Constant(binder.CallInfo.ArgumentNames, typeof(IEnumerable<string>));
            var argNamedEnumerable = Expression.Call(typeof(Arguments).GetMethod("From"), argValues, argNames);

            var binderDefault = binder.FallbackInvokeMember(this, args);

            var missingLambda = Expression.Lambda(Expression.Call(
                GetClayBehavior(),
                IClayBehavior_InvokeMemberMissing,
                Expression.Lambda(binderDefault.Expression),
                GetLimitedSelf(),
                Expression.Constant(binder.Name),
                argNamedEnumerable));

            var call = Expression.Call(
                GetClayBehavior(),
                IClayBehavior_InvokeMember,
                missingLambda,
                GetLimitedSelf(),
                Expression.Constant(binder.Name),
                argNamedEnumerable);

            var dynamicSuggestion = new DynamicMetaObject(
                call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderDefault.Restrictions));

            return binder.FallbackInvokeMember(this, args, dynamicSuggestion);
        }


        public override DynamicMetaObject BindConvert(ConvertBinder binder) {
            Trace.WriteLine("BindConvert");

            var binderDefault = binder.FallbackConvert(this);

            var missingLambda = Expression.Lambda(Expression.Call(
                GetClayBehavior(),
                IClayBehavior_ConvertMissing,
                Expression.Lambda(binderDefault.Expression),
                GetLimitedSelf(),
                Expression.Constant(binder.Type),
                Expression.Constant(binder.Explicit)));

            var call = Expression.Call(
                GetClayBehavior(),
                IClayBehavior_Convert,
                missingLambda,
                GetLimitedSelf(),
                Expression.Constant(binder.Type),
                Expression.Constant(binder.Explicit));

            var convertedCall = Expression.Convert(call, binder.ReturnType);

            var dynamicSuggestion = new DynamicMetaObject(
                convertedCall, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderDefault.Restrictions));

            return binder.FallbackConvert(this, dynamicSuggestion);
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
                IClayBehavior_GetIndex,
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
                IClayBehavior_SetIndex,
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

            var argValues = Expression.NewArrayInit(typeof(object), args.Select(x => Expression.Convert(x.Expression, typeof(Object))));
            var argNames = Expression.Constant(binder.CallInfo.ArgumentNames, typeof(IEnumerable<string>));
            var argNamedEnumerable = Expression.Call(typeof(Arguments).GetMethod("From"), argValues, argNames);

            var binderFallback = binder.FallbackInvoke(this, args);

            var call = Expression.Call(
                GetClayBehavior(),
                IClayBehavior_InvokeMember,
                Expression.Lambda(binderFallback.Expression),
                GetLimitedSelf(),
                Expression.Constant(null, typeof(string)),
                argNamedEnumerable);

            return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderFallback.Restrictions));
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

            var binderFallback = binder.FallbackBinaryOperation(this, arg);

            var call = Expression.Call(
                GetClayBehavior(),
                IClayBehavior_BinaryOperation,
                Expression.Lambda(binderFallback.Expression),
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