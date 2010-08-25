using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ClaySharp.Tests.Binders {
    [TestFixture]
    public class InvokeBinderTests {
        [Test]
        public void InvokeMemberContainsName() {
            dynamic clay = new Clay(new TestBehavior());
            var result = clay.Hello();
            Assert.That(result, Is.StringContaining("[name:Hello]"));
            Assert.That(result, Is.StringContaining("[count:0]"));
        }
        [Test]
        public void InvokeBinder() {
            dynamic clay = new Clay(new TestBehavior());
            var result = clay();
            Assert.That(result, Is.StringContaining("[name:<null>]"));
            Assert.That(result, Is.StringContaining("[count:0]"));
        }

        class TestBehavior : ClayBehavior {
            public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
                return string.Format("[name:{0}] [count:{1}]", name??"<null>", args.Count());
            }
        }
    }
}
