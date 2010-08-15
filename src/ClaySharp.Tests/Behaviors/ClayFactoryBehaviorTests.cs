using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using ClaySharp.Behaviors;
using NUnit.Framework;

namespace ClaySharp.Tests.Behaviors {
    [TestFixture]
    public class ClayFactoryBehaviorTests {
        [Test]
        public void InvokingMethodsCreateDynamicObjectWithBehaviors() {
            dynamic factory = new Clay(new ClayFactoryBehavior());
            object alpha = factory.Alpha();

            Assert.That(alpha, Is.AssignableTo<IDynamicMetaObjectProvider>());
            Assert.That(alpha, Is.AssignableTo<IClayBehaviorProvider>());

        }
    }
}
