using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CSharp.RuntimeBinder;

namespace ClaySharp.Behaviors {
    public class ClayFactoryBehavior : ClayBehavior {
        public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {

            dynamic shape = new Clay(
                new InterfaceProxyBehavior(), 
                new PropBehavior(), 
                new ArrayPropAssignmentBehavior(),
                new NilResultBehavior());

            shape.ShapeName = name;

            if (args.Count() == 1) {
                var options = args.Single();
                var assigner = GetAssigner(options.GetType());
                assigner.Invoke(shape, options);
            }

            return shape;
        }



        private static Action<dynamic, object> GetAssigner(Type sourceType) {
            lock (_assignerCache) {
                Action<dynamic, object> assigner;
                if (_assignerCache.TryGetValue(sourceType, out assigner))
                    return assigner;


                // given "sourceType T" with public properties, e.g. X and Y, generate the following lambda
                //
                // (dynamic target, object source) => { 
                //    target.X = ((T)source).X; 
                //    target.Y = ((T)source).Y;
                //  }

                var targetParameter = Expression.Parameter(typeof (object), "target");
                var sourceParameter = Expression.Parameter(typeof (object), "source");

                // for each propertyInfo, e.g. X
                // produce dynamic call site, (target).X = ((T)source).X
                var assignments = sourceType.GetProperties().Select(
                    property => Expression.Dynamic(
                        Binder.SetMember(
                            CSharpBinderFlags.None,
                            property.Name,
                            typeof (void),
                            new[] {
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                            }),
                        typeof (void),
                        targetParameter,
                        Expression.Property(
                            Expression.Convert(sourceParameter, sourceType),
                            property)));


                var lambda = Expression.Lambda<Action<dynamic, object>>(
                    Expression.Block(assignments),
                    targetParameter,
                    sourceParameter);

                assigner = lambda.Compile();
                _assignerCache.Add(sourceType, assigner);
                return assigner;
            }

        }


        static readonly Dictionary<Type, Action<dynamic, object>> _assignerCache = new Dictionary<Type, Action<dynamic, object>>();

    }
}
