using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using ClaySharp.Behaviors;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace ClaySharp.Tests.Timings {
    [TestFixture]
    public class PropertyGrabbingTimings {

        const int LoopCount=100000;

        [Test]
        public void TimingPropertyAssignmentViaPropertyReflection() {

            var time = new Stopwatch();
            time.Start();
            for (var loop = 0; loop != LoopCount; ++loop) {
                var source = new { One = 1, Two = "dos" };
                dynamic target = new Clay(new PropBehavior());
                foreach (var prop in source.GetType().GetProperties()) {
                    target[prop.Name] = prop.GetValue(source, null);
                }
                if (loop==0)
                    time.Restart();
            }
            time.Stop();

            Trace.WriteLine(string.Format("Total time {0} ms", time.ElapsedMilliseconds));
        }

        [Test]
        public void zTimingPropertyAssignmentViaCompiledExpression() {

            var time = new Stopwatch();
            time.Start();
            for (var loop = 0; loop != LoopCount; ++loop) {
                var source = new { One = 1, Two = "dos" };
                dynamic target = new Clay(new PropBehavior());
                var assignment = CompileAssignments(source.GetType());
                assignment.Invoke(target, source);
                if (loop==0)
                    time.Restart();
            }
            time.Stop();

            Trace.WriteLine(string.Format("Total time {0} ms", time.ElapsedMilliseconds));
        }

        Dictionary<Type, Action<dynamic, object>> _assigners = new Dictionary<Type, Action<dynamic, object>>();

        private Action<dynamic, object> CompileAssignments(Type sourceType) {
            lock (_assigners) {
                Action<dynamic, object> value;
                if (_assigners.TryGetValue(sourceType, out value))
                    return value;


                // given "sourceType T" with public properties, e.g. X and Y, generate the following lambda
                //
                // (dynamic target, object source) => { 
                //    target.X = ((T)source).X; 
                //    target.Y = ((T)source).Y;
                //  }

                var targetParameter = Expression.Parameter(typeof(object), "target");
                var sourceParameter = Expression.Parameter(typeof(object), "source");

                var assignments = sourceType.GetProperties().Select(
                    propertyInfo => {
                        // for each propertyInfo, e.g. X

                        // sourceValue is clr expression ((T)source).X
                        var sourceValue = Expression.Property(Expression.Convert(sourceParameter, sourceType), propertyInfo);


                        // produce dynamic call site, (target).X = (sourceValue))
                        return Expression.Dynamic(
                            Binder.SetMember(
                                CSharpBinderFlags.None,
                                propertyInfo.Name,
                                targetParameter.Type,
                                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }),
                            typeof(void),
                            targetParameter,
                            sourceValue);
                        
                    });


                var lambda = Expression.Lambda<Action<dynamic, object>>(
                    Expression.Block(assignments),
                    targetParameter,
                    sourceParameter);

                value = lambda.Compile();
                _assigners.Add(sourceType, value);
                return value;
            }

        }


        [Test]
        public void TimingPropertyAssignmentViaCompiledIndexers() {

            var time = new Stopwatch();
            time.Start();
            for (var loop = 0; loop != LoopCount; ++loop) {
                var source = new { One = 1, Two = "dos" };
                dynamic target = new Clay(new PropBehavior());
                var assignment = CompileAssignments2(source.GetType());
                assignment.Invoke(target, source);
                if (loop==0)
                    time.Restart();
            }
            time.Stop();

            Trace.WriteLine(string.Format("Total time {0} ms", time.ElapsedMilliseconds));
        }

        Dictionary<Type, Action<dynamic, object>> _assigners2 = new Dictionary<Type, Action<dynamic, object>>();

        private Action<dynamic, object> CompileAssignments2(Type sourceType) {
            lock (_assigners2) {
                Action<dynamic, object> value;
                if (_assigners2.TryGetValue(sourceType, out value))
                    return value;


                // given "sourceType T" with public properties, e.g. X and Y, generate the following lambda
                //
                // (dynamic target, object source) => { 
                //    target["X"] = ((T)source).X; 
                //    target["Y"] = ((T)source).Y;
                //  }

                var targetParameter = Expression.Parameter(typeof(object), "target");
                var sourceParameter = Expression.Parameter(typeof(object), "source");

                var assignments = sourceType.GetProperties().Select(
                    propertyInfo => {
                        // for each propertyInfo, e.g. X

                        // sourceValue is clr expression ((T)source).X
                        var sourceValue = Expression.Property(Expression.Convert(sourceParameter, sourceType), propertyInfo);

                        // produce dynamic call site, (target)["X"] = (sourceValue))
                        return Expression.Dynamic(
                            Binder.SetIndex(
                                CSharpBinderFlags.None,                                
                                targetParameter.Type,
                                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }),
                            typeof(void),
                            targetParameter,
                            Expression.Constant(propertyInfo.Name, typeof(string)),
                            sourceValue);
                    });


                var lambda = Expression.Lambda<Action<dynamic, object>>(
                    Expression.Block(assignments),
                    targetParameter,
                    sourceParameter);

                value = lambda.Compile();
                _assigners2.Add(sourceType, value);
                return value;
            }

        }
    }
}
