using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ClaySharp.Behaviors;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace ClaySharp.Tests.Behaviors {
    [TestFixture]
    public class ClayFactoryBehaviorTests {
        [Test]
        public void InvokingMethodsCreateDynamicObjectWithBehaviors() {
            dynamic factory = new Clay(new ClayFactoryBehavior());
            object alpha = factory.Alpha();

            Assert.That(alpha, Is.Not.Null);
            Assert.That(alpha, Is.AssignableTo<IDynamicMetaObjectProvider>());
            Assert.That(alpha, Is.AssignableTo<IClayBehaviorProvider>());
        }

        [Test]
        public void DifferentInstanceCreatedEachCall() {
            dynamic factory = new Clay(new ClayFactoryBehavior());
            object alpha1 = factory.Alpha();
            object alpha2 = factory.Alpha();

            Assert.That(alpha1, Is.Not.SameAs(alpha2));
        }

        [Test]
        public void FactoryMethodCopiesPropertiesOfOptionalArgument() {
            dynamic factory = new Clay(new ClayFactoryBehavior());
            var alpha = factory.Alpha(new { One = 1, Two = "dos" });
            Assert.That(alpha.One, Is.EqualTo(1));
            Assert.That(alpha.Two, Is.EqualTo("dos"));
        }
    }
}
