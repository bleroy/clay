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
            ClayPlus statically = (ClayPlus)alpha;
            IClayPlus interfacially = alpha;

            Assert.That(dynamically.Hello, Is.EqualTo("World"));
            Assert.That(statically.Hello, Is.EqualTo("World"));
            //Assert.That(interfacially.Hello, Is.EqualTo("World"));

            Assert.That(dynamically.Add(3, 4), Is.EqualTo(7));
            Assert.That(statically.Add(3, 4), Is.EqualTo(7));            
            Assert.That(interfacially.Add(3, 4), Is.EqualTo(7));
            Assert.That(interfacially.Add(3, 5), Is.EqualTo(8));
            Assert.That(interfacially.Add(3, 6), Is.EqualTo(9));
        }


        public class Anything {

            public virtual string Hello { get { return "World"; } }

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

    }
}
