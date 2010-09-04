using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClaySharp.Behaviors;
using NUnit.Framework;

namespace ClaySharp.Tests {
    [TestFixture]
    public class DefaultClayActivatorTests {
        private IClayActivator _activator;

        [SetUp]
        public void Init() {
            _activator = new DefaultClayActivator();
            ClayActivator.ServiceLocator = () => _activator;
        }

        [Test]
        public void SimpleActivationUsesDefaultClass() {
            var alpha = ClayActivator.CreateInstance(Enumerable.Empty<IClayBehavior>());
            var type = alpha.GetType();

            Assert.That(type, Is.EqualTo(typeof(Clay)));
        }

        public class ClayPlus : Clay {
            public ClayPlus(IEnumerable<IClayBehavior> behaviors)
                : base(behaviors) {
            }

            public virtual string Hello { get { return "World"; } }

            public virtual int Add(int left, int right) {
                return left + right;
            }
        }

        public interface IClayPlus {
            string Hello { get; }
            int Add(int left, int right);
        }

        [Test]
        public void ClaySubclassIsActivatedWithoutDynamicProxy() {
            var alpha = ClayActivator.CreateInstance<ClayPlus>(Enumerable.Empty<IClayBehavior>());
            var type = alpha.GetType();

            Assert.That(type, Is.EqualTo(typeof(ClayPlus)));
        }

        [Test]
        public void SubclassMembersRemainAvailableStaticallyAndDynamicallyAndViaInterface() {

            var alpha = ClayActivator.CreateInstance<ClayPlus>(new[] { new InterfaceProxyBehavior() });

            dynamic dynamically = alpha;
            ClayPlus statically = alpha;
            IClayPlus interfacially = alpha;

            Assert.That(dynamically.Hello, Is.EqualTo("World"));
            Assert.That(statically.Hello, Is.EqualTo("World"));
            Assert.That(interfacially.Hello, Is.EqualTo("World"));

            Assert.That(dynamically.Add(3, 4), Is.EqualTo(7));
            Assert.That(statically.Add(3, 4), Is.EqualTo(7));
            Assert.That(interfacially.Add(3, 4), Is.EqualTo(7));
            Assert.That(interfacially.Add(3, 5), Is.EqualTo(8));
            Assert.That(interfacially.Add(3, 6), Is.EqualTo(9));
        }


        public class Anything {
            private readonly string _helloText;

            public Anything() {
            }
            public Anything(string helloText) {
                _helloText = helloText;
            }

            public virtual string Hello { get { return _helloText ?? "World"; } }

            public virtual int Add(int left, int right) {
                return left + right;
            }
        }

        [Test]
        public void ClaySubclassFromAnythingIsActivatedDynamixProxyAddingDlrInterfaces() {
            var alpha = ClayActivator.CreateInstance<Anything>(Enumerable.Empty<IClayBehavior>());

            var type = alpha.GetType();

            Assert.That(type, Is.Not.EqualTo(typeof(Anything)));
            Assert.That(typeof(Anything).IsAssignableFrom(type));

        }

        [Test]
        public void SubclassFromAnythingMembersRemainAvailableStaticallyAndDynamicallyAndViaInterface() {
            var alpha = ClayActivator.CreateInstance<Anything>(new[] { new InterfaceProxyBehavior() });
            var type = alpha.GetType();
            Assert.That(type, Is.Not.EqualTo(typeof(Anything)));

            dynamic dynamically = alpha;
            Anything statically = alpha;
            IClayPlus interfacially = alpha;

            Assert.That(dynamically.Hello, Is.EqualTo("World"));
            Assert.That(statically.Hello, Is.EqualTo("World"));
            Assert.That(interfacially.Hello, Is.EqualTo("World"));

            Assert.That(dynamically.Add(3, 4), Is.EqualTo(7));
            Assert.That(statically.Add(3, 4), Is.EqualTo(7));
            Assert.That(interfacially.Add(3, 4), Is.EqualTo(7));
            Assert.That(interfacially.Add(3, 5), Is.EqualTo(8));
            Assert.That(interfacially.Add(3, 6), Is.EqualTo(9));
        }


        [Test]
        public void BehaviorsCanFilterVirtualMethods() {

            var alpha = ClayActivator.CreateInstance<Anything>(new IClayBehavior[] { 
                new InterfaceProxyBehavior(), 
                new AnythingModifier() });

            dynamic dynamically = alpha;
            Anything statically = alpha;
            IClayPlus interfacially = alpha;

            Assert.That(dynamically.Hello, Is.EqualTo("[World]"));
            //Assert.That(statically.Hello, Is.EqualTo("[World]"));
            Assert.That(interfacially.Hello, Is.EqualTo("[World]"));

            Assert.That(dynamically.Add(3, 4), Is.EqualTo(9));
            //Assert.That(statically.Add(3, 4), Is.EqualTo(9));
            Assert.That(interfacially.Add(3, 4), Is.EqualTo(9));
            Assert.That(interfacially.Add(3, 5), Is.EqualTo(10));
            Assert.That(interfacially.Add(3, 6), Is.EqualTo(11));
        }

        class AnythingModifier : ClayBehavior {
            public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
                if (name == "Add") {
                    return (int)proceed() + 2;
                }
                return proceed();
            }
            public override object GetMember(Func<object> proceed, object self, string name) {
                if (name == "Hello") {
                    return "[" + proceed() + "]";
                }

                return proceed();
            }

        }


    }
}
