using System;
using System.Linq;
using Castle.Core.Interceptor;

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
                    return;
                }
            }
            invocation.Proceed();
        }
    }
}