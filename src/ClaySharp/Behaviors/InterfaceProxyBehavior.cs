using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace ClaySharp.Behaviors {
    public class InterfaceProxyBehavior : ClayBehavior {
        private static readonly IProxyBuilder ProxyBuilder = new DefaultProxyBuilder();
        static readonly MethodInfo DynamicMetaObjectProviderGetMetaObject = typeof(IDynamicMetaObjectProvider).GetMethod("GetMetaObject");

        public override object ConvertMissing(Func<object> proceed, object self, Type type, bool isExplicit) {
            if (type.IsInterface && type != typeof(IDynamicMetaObjectProvider)) {
                var proxyType = ProxyBuilder.CreateInterfaceProxyTypeWithoutTarget(
                    type,
                    new[] { typeof(IDynamicMetaObjectProvider) },
                    ProxyGenerationOptions.Default);

                var interceptors = new IInterceptor[] { new Interceptor(self) };
                var proxy = Activator.CreateInstance(proxyType, new object[] { interceptors, self });
                return proxy;
            }

            return proceed();
        }

        class Interceptor : IInterceptor {
            public object Self { get; private set; }

            public Interceptor(object self) {
                Self = self;
            }

            public void Intercept(IInvocation invocation) {
                if (invocation.Method == DynamicMetaObjectProviderGetMetaObject) {
                    var expression = (Expression)invocation.Arguments.Single();
                    invocation.ReturnValue = new ForwardingMetaObject(
                        expression,
                        BindingRestrictions.Empty,
                        invocation.Proxy,
                        (IDynamicMetaObjectProvider)Self,
                        exprProxy => Expression.Field(exprProxy, "__target"));

                    return;
                }

                var invoker = BindInvoker(invocation);
                invoker(invocation);

                if (invocation.ReturnValue != null &&
                    !invocation.Method.ReturnType.IsAssignableFrom(invocation.ReturnValue.GetType()) &&
                    invocation.ReturnValue is IClayBehaviorProvider) {

                    var returnValueBehavior = ((IClayBehaviorProvider) invocation.ReturnValue).Behavior;
                    invocation.ReturnValue = returnValueBehavior.Convert(
                        () => returnValueBehavior.ConvertMissing(
                            () => invocation.ReturnValue,
                            invocation.ReturnValue,
                            invocation.Method.ReturnType,
                            false),
                        invocation.ReturnValue,
                        invocation.Method.ReturnType,
                        false);
                }
            }


            static readonly ConcurrentDictionary<MethodInfo, Action<IInvocation>> Invokers = new ConcurrentDictionary<MethodInfo, Action<IInvocation>>();

            private static Action<IInvocation> BindInvoker(IInvocation invocation) {
                return Invokers.GetOrAdd(invocation.Method, CompileInvoker);
            }

            private static Action<IInvocation> CompileInvoker(MethodInfo method) {

                var methodParameters = method.GetParameters();
                var invocationParameter = Expression.Parameter(typeof(IInvocation), "invocation");

                var targetAndArgumentInfos = Pack(
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    methodParameters.Select(
                                        mp => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.NamedArgument, mp.Name)));

                var targetAndArguments = Pack<Expression>(
                    Expression.Property(invocationParameter, invocationParameter.Type, "InvocationTarget"),
                    methodParameters.Select(
                        (mp, index) =>
                            Expression.Convert(
                                Expression.ArrayIndex(
                                    Expression.Property(invocationParameter, invocationParameter.Type,
                                        "Arguments"),
                                    Expression.Constant(index)), mp.ParameterType)));

                Expression body = null;
                if (method.IsSpecialName) {
                    if (body == null && method.Name.Equals("get_Item")) {
                        body = Expression.Dynamic(
                            Binder.GetIndex(
                                CSharpBinderFlags.InvokeSpecialName,
                                typeof(object),
                                targetAndArgumentInfos),
                            typeof(object),
                            targetAndArguments);
                    }

                    if (body == null && method.Name.Equals("set_Item")) {

                        var targetAndArgumentInfosWithoutTheNameValue = Pack(
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                            methodParameters.Select(
                                            mp => mp.Name == "value" ? CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) : CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.NamedArgument, mp.Name)));

                        body = Expression.Dynamic(
                            Binder.SetIndex(
                                CSharpBinderFlags.InvokeSpecialName,
                                typeof(object),
                                targetAndArgumentInfosWithoutTheNameValue),
                            typeof(object),
                            targetAndArguments);
                    }

                    if (body == null && method.Name.StartsWith("get_")) {
                        //  Build lambda containing the following call site:
                        //  (IInvocation invocation) => {
                        //      invocation.ReturnValue = (object) ((dynamic)invocation.InvocationTarget).{method.Name};
                        //  }
                        body = Expression.Dynamic(
                            Binder.GetMember(
                                CSharpBinderFlags.InvokeSpecialName,
                                method.Name.Substring("get_".Length),
                                typeof(object),
                                targetAndArgumentInfos),
                            typeof(object),
                            targetAndArguments);
                    }

                    if (body == null && method.Name.StartsWith("set_")) {
                        body = Expression.Dynamic(
                            Binder.SetMember(
                                CSharpBinderFlags.InvokeSpecialName,
                                method.Name.Substring("set_".Length),
                                typeof(object),
                                targetAndArgumentInfos),
                            typeof(object),
                            targetAndArguments);
                    }
                }
                if (body == null) {
                    //  Build lambda containing the following call site:
                    //  (IInvocation invocation) => {
                    //      invocation.ReturnValue = (object) ((dynamic)invocation.InvocationTarget).{method.Name}(
                    //          {methodParameters[*].Name}: ({methodParameters[*].Type})invocation.Arguments[*],
                    //          ...);
                    //  }


                    body = Expression.Dynamic(
                        Binder.InvokeMember(
                            CSharpBinderFlags.None,
                            method.Name,
                            null,
                            typeof(object),
                            targetAndArgumentInfos),
                        typeof(object),
                        targetAndArguments);
                }

                if (body != null && method.ReturnType != typeof(void)) {
                    body = Expression.Assign(
                        Expression.Property(invocationParameter, invocationParameter.Type, "ReturnValue"),
                        Expression.Convert(body, typeof(object)));
                }

                var lambda = Expression.Lambda<Action<IInvocation>>(body, invocationParameter);
                return lambda.Compile();
            }

        }

        static IEnumerable<T> Pack<T>(T t1) {
            if (!Equals(t1, default(T)))
                yield return t1;
        }
        static IEnumerable<T> Pack<T>(T t1, IEnumerable<T> t2) {
            if (!Equals(t1, default(T)))
                yield return t1;
            foreach (var t in t2)
                yield return t;
        }
        static IEnumerable<T> Pack<T>(T t1, IEnumerable<T> t2, T t3) {
            if (!Equals(t1, default(T)))
                yield return t1;
            foreach (var t in t2)
                yield return t;
            if (!Equals(t3, default(T)))
                yield return t3;
        }

        /// <summary>
        /// Based on techniques discussed by Tomáš Matoušek
        /// at http://blog.tomasm.net/2009/11/07/forwarding-meta-object/
        /// </summary>
        public sealed class ForwardingMetaObject : DynamicMetaObject {
            private readonly DynamicMetaObject _metaForwardee;

            public ForwardingMetaObject(Expression expression, BindingRestrictions restrictions, object forwarder,
                IDynamicMetaObjectProvider forwardee, Func<Expression, Expression> forwardeeGetter)
                : base(expression, restrictions, forwarder) {

                // We'll use forwardee's meta-object to bind dynamic operations.
                _metaForwardee = forwardee.GetMetaObject(
                    forwardeeGetter(
                        Expression.Convert(expression, forwarder.GetType())   // [1]
                    )
                );
            }

            // Restricts the target object's type to TForwarder. 
            // The meta-object we are forwarding to assumes that it gets an instance of TForwarder (see [1]).
            // We need to ensure that the assumption holds.
            private DynamicMetaObject AddRestrictions(DynamicMetaObject result) {
                var restricted = new DynamicMetaObject(
                    result.Expression,
                    BindingRestrictions.GetTypeRestriction(Expression, Value.GetType()).Merge(result.Restrictions),
                    _metaForwardee.Value
                    );
                return restricted;
            }

            // Forward all dynamic operations or some of them as needed //

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
                return AddRestrictions(_metaForwardee.BindGetMember(binder));
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
                return AddRestrictions(_metaForwardee.BindSetMember(binder, value));
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
                return AddRestrictions(_metaForwardee.BindDeleteMember(binder));
            }

            public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
                return AddRestrictions(_metaForwardee.BindGetIndex(binder, indexes));
            }

            public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
                return AddRestrictions(_metaForwardee.BindSetIndex(binder, indexes, value));
            }

            public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes) {
                return AddRestrictions(_metaForwardee.BindDeleteIndex(binder, indexes));
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
                return AddRestrictions(_metaForwardee.BindInvokeMember(binder, args));
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
                return AddRestrictions(_metaForwardee.BindInvoke(binder, args));
            }

            public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args) {
                return AddRestrictions(_metaForwardee.BindCreateInstance(binder, args));
            }

            public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder) {
                return AddRestrictions(_metaForwardee.BindUnaryOperation(binder));
            }

            public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg) {
                return AddRestrictions(_metaForwardee.BindBinaryOperation(binder, arg));
            }

            public override DynamicMetaObject BindConvert(ConvertBinder binder) {
                return AddRestrictions(_metaForwardee.BindConvert(binder));
            }


        }
    }
}
