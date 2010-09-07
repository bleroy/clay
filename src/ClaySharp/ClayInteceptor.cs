using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using Castle.Core.Interceptor;
using ClaySharp.Implementation;
using Microsoft.CSharp.RuntimeBinder;

namespace ClaySharp {
    public class ClayInteceptor : IInterceptor {
        private const string GetPrefix = "get_";
        private const string SetPrefix = "set_";

        public void Intercept(IInvocation invocation) {
            var invocationDestinedForSelf = ReferenceEquals(invocation.InvocationTarget, invocation.Proxy);
            if (!invocationDestinedForSelf) {
                // don't intercept mixins
                invocation.Proceed();
                return;
            }

            var behaviorProvider = invocation.Proxy as IClayBehaviorProvider;
            if (behaviorProvider != null) {
                var invocationMethod = invocation.Method;
                if (invocationMethod.IsSpecialName &&
                    invocationMethod.Name.StartsWith(GetPrefix)) {
                    invocation.ReturnValue = behaviorProvider.Behavior.GetMember(
                        () => {
                            invocation.Proceed();
                            return invocation.ReturnValue;
                        },
                        invocation.Proxy,
                        invocationMethod.Name.Substring(GetPrefix.Length));
                    AdjustReturnValue(invocation);
                    return;
                }
                if (invocationMethod.IsSpecialName &&
                    invocationMethod.Name.StartsWith(SetPrefix) &&
                    invocation.Arguments.Count() == 1) {
                    invocation.ReturnValue = behaviorProvider.Behavior.SetMember(
                        () => {
                            invocation.Proceed();
                            return invocation.ReturnValue;
                        },
                        invocation.Proxy,
                        invocationMethod.Name.Substring(SetPrefix.Length),
                        invocation.Arguments.Single());
                    AdjustReturnValue(invocation);
                    return;
                }

                if (!invocationMethod.IsSpecialName) {
                    invocation.ReturnValue = behaviorProvider.Behavior.InvokeMember(
                        () => { invocation.Proceed(); return invocation.ReturnValue; },
                        invocation.Proxy,
                        invocationMethod.Name,
                        Arguments.From(invocation.Arguments, Enumerable.Empty<string>()));
                    AdjustReturnValue(invocation);
                    return;
                }
            }
            invocation.Proceed();
        }

        static readonly ConcurrentDictionary<Type, CallSite<Func<CallSite, object, object>>> _convertSites = new ConcurrentDictionary<Type, CallSite<Func<CallSite, object, object>>>();

        private static void AdjustReturnValue(IInvocation invocation) {
            var methodReturnType = invocation.Method.ReturnType;
            if (methodReturnType == typeof(void))
                return;

            if (invocation.ReturnValue == null)
                return;

            var returnValueType = invocation.ReturnValue.GetType();
            if (methodReturnType.IsAssignableFrom(returnValueType))
                return;

            var callSite = _convertSites.GetOrAdd(
                methodReturnType,
                x => CallSite<Func<CallSite, object, object>>.Create(
                    Binder.Convert(CSharpBinderFlags.None, x, null)));

            invocation.ReturnValue = callSite.Target(callSite, invocation.ReturnValue);
        }
    }
}