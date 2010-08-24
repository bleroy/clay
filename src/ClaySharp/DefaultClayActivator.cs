using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;

namespace ClaySharp {
    public class DefaultClayActivator : IClayActivator {
        static IProxyBuilder _builder = new DefaultProxyBuilder();

        public dynamic CreateInstance<TBase>(IEnumerable<IClayBehavior> behaviors) {
            var baseType = typeof(TBase);
            var isDynamicMetaObjectProvider = typeof(IDynamicMetaObjectProvider).IsAssignableFrom(baseType);
            var isClayBehaviorProvider = typeof(IClayBehaviorProvider).IsAssignableFrom(baseType);

            if (isDynamicMetaObjectProvider && isClayBehaviorProvider) {
                var constructorArguments = new object[] { behaviors };
                return Activator.CreateInstance(baseType, constructorArguments);
            }

            Func<object, object> contextualize = proxy => proxy;

            var options = new ProxyGenerationOptions();
            var constructorArgs = new List<object>();
            if (!isClayBehaviorProvider) {
                var mixin = new MixinClayBehaviorProvider(behaviors);
                options.AddMixinInstance(mixin);
                constructorArgs.Add(mixin);
            }
            if (!isDynamicMetaObjectProvider) {
                var mixin = new MixinDynamicMetaObjectProvider();
                options.AddMixinInstance(mixin);
                constructorArgs.Add(mixin);
                var prior = contextualize;
                contextualize = proxy => { mixin.Instance = proxy; return prior(proxy); };
            }

            var proxyType = _builder.CreateClassProxy(baseType, options);

            constructorArgs.Add(new IInterceptor[0]);

            return contextualize(Activator.CreateInstance(proxyType, constructorArgs.ToArray()));
        }

        class MixinClayBehaviorProvider : IClayBehaviorProvider {
            private readonly IClayBehavior _behavior;

            public MixinClayBehaviorProvider(IEnumerable<IClayBehavior> behaviors) {
                _behavior = new ClayBehaviorCollection(behaviors);
            }

            IClayBehavior IClayBehaviorProvider.Behavior {
                get { return _behavior; }
            }
        }

        class MixinDynamicMetaObjectProvider : IDynamicMetaObjectProvider {
            public object Instance { get; set; }

            DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression expression) {
                return new ClayMetaObject(Instance, expression);
            }
        }
    }
}
