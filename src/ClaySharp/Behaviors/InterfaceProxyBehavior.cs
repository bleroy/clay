using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using ClaySharp.Implementation;
using Microsoft.CSharp.RuntimeBinder;

namespace ClaySharp.Behaviors {
    public class InterfaceProxyBehavior : ClayBehavior {
        private static readonly IProxyBuilder ProxyBuilder = new DefaultProxyBuilder();
        static readonly MethodInfo DynamicMetaObjectProviderGetMetaObject = typeof(IDynamicMetaObjectProvider).GetMethod("GetMetaObject");

        public override object Convert(Func<object> proceed, object self, Type type, bool isExplicit) {
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

                var behavior = ((IClayBehaviorProvider)invocation.InvocationTarget).Behavior;

                //var invoker = BindInvoker(invocation);
                //invoker(invocation);

                Func<object> proceed = () => {
                    invocation.Proceed();
                    return invocation.ReturnValue;
                };

                if (invocation.Method.Name.Equals("get_Item")) {
                    invocation.ReturnValue = behavior.GetIndex(
                        proceed,
                        invocation.Arguments);
                }
                else if (invocation.Method.Name.Equals("set_Item")) {
                    invocation.ReturnValue = behavior.SetIndex(
                        proceed,
                        invocation.Arguments.Take(invocation.Arguments.Count() - 1),
                        invocation.Arguments.Last());
                }
                else if (!invocation.Arguments.Any() && invocation.Method.Name.StartsWith("get_")) {
                    invocation.ReturnValue = behavior.GetMember(
                        proceed,
                        invocation.Method.Name.Substring("get_".Length));
                }
                else if (invocation.Arguments.Count() == 1 && invocation.Method.Name.StartsWith("set_")) {
                    invocation.ReturnValue = behavior.SetMember(
                        proceed,
                        invocation.Method.Name.Substring("set_".Length),
                        invocation.Arguments.Single());
                }
                else {
                    var invoker = BindInvoker(invocation);
                    invoker(invocation);

                    //invocation.ReturnValue = behavior.InvokeMember(
                    //    proceed,
                    //    invocation.InvocationTarget,
                    //    invocation.Method.Name,
                    //    Arguments.From(invocation.Arguments, invocation.Method.GetParameters().Select(parameter => parameter.Name)));
                }

                if (invocation.ReturnValue != null &&
                    !invocation.Method.ReturnType.IsAssignableFrom(invocation.ReturnValue.GetType()) &&
                    invocation.ReturnValue is IClayBehaviorProvider) {

                    var returnValueBehavior = ((IClayBehaviorProvider)invocation.ReturnValue).Behavior;
                    invocation.ReturnValue = returnValueBehavior.Convert(
                        () => invocation.ReturnValue,
                        invocation.ReturnValue,
                        invocation.Method.ReturnType,
                        false);
                }
            }


            static readonly ConcurrentDictionary<MethodInfo, Action<IInvocation>> _invokers = new ConcurrentDictionary<MethodInfo, Action<IInvocation>>();

            private static Action<IInvocation> BindInvoker(IInvocation invocation) {
                return _invokers.GetOrAdd(invocation.Method, CompileInvoker);
            }

            private static Action<IInvocation> CompileInvoker(MethodInfo method) {
                var methodParameters = method.GetParameters();
                var invocationParameter = Expression.Parameter(typeof(IInvocation), "invocation");
                Expression body;
                if (method.Name.Equals("get_Item")) {
                    throw new NotImplementedException();
                }
                else if (method.Name.Equals("set_Item")) {
                    throw new NotImplementedException();
                }
                else if (method.Name.StartsWith("get_")) {
                    throw new NotImplementedException();
                }
                else if (method.Name.EndsWith("set_")) {
                    throw new NotImplementedException();
                }
                else {

                    var binder = Microsoft.CSharp.RuntimeBinder.Binder.InvokeMember(
                        CSharpBinderFlags.None,
                        method.Name,
                        null,//methodParameters.Select(p => p.ParameterType),
                        typeof(object),
                        new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }.Concat(
                        methodParameters.Select(
                            mp => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.NamedArgument, mp.Name))));

                    body = Expression.Dynamic(binder,
                        typeof(object),//method.ReturnType,
                        new Expression[] { Expression.Property(invocationParameter, invocationParameter.Type, "InvocationTarget") }.Concat(
                        methodParameters.Select(
                            (mp, index) =>
                                Expression.Convert(
                                    Expression.ArrayIndex(
                                        Expression.Property(invocationParameter, invocationParameter.Type, "Arguments"),
                                        Expression.Constant(index)), mp.ParameterType))));

                    if (method.ReturnType != typeof(void)) {
                        body = Expression.Assign(
                            Expression.Property(invocationParameter, invocationParameter.Type, "ReturnValue"),
                            Expression.Convert(body, typeof(object)));
                    }

                }
                var lambda = Expression.Lambda<Action<IInvocation>>(body, invocationParameter);
                return lambda.Compile();
                //if (method.Name.Equals("get_Item")) {
                //    invocation.ReturnValue = behavior.GetIndex(
                //        proceed,
                //        invocation.Arguments);
                //}
                //else if (method.Name.Equals("set_Item")) {
                //    invocation.ReturnValue = behavior.SetIndex(
                //        proceed,
                //        invocation.Arguments.Take(invocation.Arguments.Count() - 1),
                //        invocation.Arguments.Last());
                //}
                //else if (!invocation.Arguments.Any() && invocation.Method.Name.StartsWith("get_")) {
                //    invocation.ReturnValue = behavior.GetMember(
                //        proceed,
                //        invocation.Method.Name.Substring("get_".Length));
                //}
                //else if (invocation.Arguments.Count() == 1 && invocation.Method.Name.StartsWith("set_")) {
                //    invocation.ReturnValue = behavior.SetMember(
                //        proceed,
                //        invocation.Method.Name.Substring("set_".Length),
                //        invocation.Arguments.Single());
                //}
                //else {
                //    invocation.ReturnValue = behavior.InvokeMember(
                //        proceed,
                //        invocation.InvocationTarget,
                //        invocation.Method.Name,
                //        Arguments.From(invocation.Arguments, invocation.Method.GetParameters().Select(parameter => parameter.Name)));
                //}
            }
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
