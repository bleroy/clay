using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClaySharp.Behaviors;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;

namespace ClaySharp.Tests {
    [TestFixture]
    public class BinderFallbackTests {

        class TestMemberBehavior : ClayBehavior {
            public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
                return name == "Sample" ? "Data" : proceed();
            }
            public override object GetMember(Func<object> proceed, object self, string name) {
                return name == "Sample" ? "Data" : proceed();
            }
            public override object SetMember(Func<object> proceed, object self, string name, object value) {
                return name == "Sample" ? "Data" : proceed();
            }
        }

        class TestIndexBehavior : ClayBehavior {
            public override object GetIndex(Func<object> proceed, object self, IEnumerable<object> keys) {
                return IsIndexZero(keys) ? "Data" : proceed();
            }

            public override object SetIndex(Func<object> proceed, object self, IEnumerable<object> keys, object value) {
                return IsIndexZero(keys) ? "Data" : proceed();
            }

            private static bool IsIndexZero(IEnumerable<object> keys) {
                return keys.Count() == 1
                    && keys.Single().GetType() == typeof(int)
                    && keys.Cast<int>().Single() == 0;
            }
        }


        [Test]
        public void InvokeMemberThrowsFallbackException() {
            dynamic alpha = new Object();
            dynamic beta = new Clay(new TestMemberBehavior());

            var ex1 = Assert.Throws<RuntimeBinderException>(() => alpha.Hello1());

            Assert.That(ex1.Message, Is.StringEnding("does not contain a definition for 'Hello1'"));

            var ex2 = Assert.Throws<RuntimeBinderException>(() => beta.Hello2());

            Assert.That(ex2.Message, Is.StringEnding("does not contain a definition for 'Hello2'"));

            Assert.That(beta.Sample(), Is.EqualTo("Data"));

        }


        [Test]
        public void GetMemberThrowsFallbackException() {
            dynamic alpha = new Object();
            dynamic beta = new Clay(new TestMemberBehavior());

            var ex1 = Assert.Throws<RuntimeBinderException>(() => { var hi = alpha.Hello1; });

            Assert.That(ex1.Message, Is.StringEnding("does not contain a definition for 'Hello1'"));

            var ex2 = Assert.Throws<RuntimeBinderException>(() => { var hi = beta.Hello2; });

            Assert.That(ex2.Message, Is.StringEnding("does not contain a definition for 'Hello2'"));

            Assert.That(beta.Sample, Is.EqualTo("Data"));
        }

        [Test]
        public void SetMemberThrowsFallbackException() {
            dynamic alpha = new Object();
            dynamic beta = new Clay(new TestMemberBehavior());

            var ex1 = Assert.Throws<RuntimeBinderException>(() => { alpha.Hello1 = 1; });

            Assert.That(ex1.Message, Is.StringEnding("does not contain a definition for 'Hello1'"));

            var ex2 = Assert.Throws<RuntimeBinderException>(() => { beta.Hello2 = 2; });

            Assert.That(ex2.Message, Is.StringEnding("does not contain a definition for 'Hello2'"));

            var x = (beta.Sample = 3);
            Assert.That(x, Is.EqualTo("Data"));
        }


        [Test]
        public void GetIndexThrowsFallbackException() {
            dynamic alpha = new Object();
            dynamic beta = new Clay(new TestMemberBehavior());

            var ex1 = Assert.Throws<RuntimeBinderException>(() => { var hi = alpha[0]; });
            Assert.That(ex1.Message, Is.StringMatching(@"Cannot apply indexing with \[\] to an expression of type .*"));

            var ex2 = Assert.Throws<RuntimeBinderException>(() => { var hi = beta[0]; });
            Assert.That(ex2.Message, Is.StringMatching(@"Cannot apply indexing with \[\] to an expression of type .*"));
        }


        [Test]
        public void SetIndexThrowsFallbackException() {
            dynamic alpha = new Object();
            dynamic beta = new Clay(new TestMemberBehavior());

            var ex1 = Assert.Throws<RuntimeBinderException>(() => { alpha[0] = 1; });

            Assert.That(ex1.Message, Is.StringMatching(@"Cannot apply indexing with \[\] to an expression of type .*"));

            var ex2 = Assert.Throws<RuntimeBinderException>(() => { beta[0] = 2; });

            Assert.That(ex2.Message, Is.StringMatching(@"Cannot apply indexing with \[\] to an expression of type .*"));

        }

        public interface IAlpha {
            string Hello();
            string Foo();
        }
        public class Alpha {
            public virtual string Hello() {
                return "World";
            }
        }

        public class AlphaBehavior : ClayBehavior {
            public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
                return proceed() + "-";
            }
            public override object InvokeMemberMissing(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
                if (name == "Foo")
                    return "Bar";
                return proceed();
            }
        }

        [Test]
        public void TestInvokePaths() {
            var dynamically = ClayActivator.CreateInstance<Alpha>(new IClayBehavior[] { 
                new InterfaceProxyBehavior(), 
                new AlphaBehavior() 
            });
            Alpha statically = dynamically;
            IAlpha interfacially = dynamically;

            Assert.That(dynamically.Hello(), Is.EqualTo("World-"));
            Assert.That(statically.Hello(), Is.EqualTo("World-"));
            Assert.That(interfacially.Hello(), Is.EqualTo("World-"));

            Assert.That(dynamically.Foo(), Is.EqualTo("Bar-"));
            Assert.That(interfacially.Foo(), Is.EqualTo("Bar-"));

            Assert.Throws<RuntimeBinderException>(() => dynamically.MissingNotHandled());
        }


        public class Beta {
            public virtual string Hello {
                get { return "World"; }
            }
        }

        public class BetaBehavior : ClayBehavior {
            public override object GetMember(Func<object> proceed, object self, string name) {
                return proceed() + "-";
            }
            public override object GetMemberMissing(Func<object> proceed, object self, string name) {
                if (name == "Foo")
                    return "Bar";
                return proceed();
            }
        }

        [Test]
        public void TestGetPaths() {
            var dynamically = ClayActivator.CreateInstance<Beta>(new[] { new BetaBehavior() });
            Beta statically = dynamically;

            Assert.That(dynamically.Hello, Is.EqualTo("World-"));
            Assert.That(statically.Hello, Is.EqualTo("World-"));

            Assert.That(dynamically.Foo, Is.EqualTo("Bar-"));

            Assert.Throws<RuntimeBinderException>(() => { var x = dynamically.MissingPropNotHandled; });
        }
    }
}
