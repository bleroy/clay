using System;
using System.Collections.Generic;
using System.Linq;
using ClaySharp.Behaviors;
using NUnit.Framework;

namespace ClaySharp.Tests.Behaviors {
    [TestFixture]
    public class ArrayPropAssignmentBehaviorTests {
        [Test]
        public void SingleArrayArgumentBecomesDynamic() {
            dynamic alpha = new Clay(new PropBehavior(), new ArrayPropAssignmentBehavior());

            alpha
                .Names(new[] { "foo", "bar", "quad" })
                .Places(new int[0]);

            alpha.Names.Add("quux");
            alpha.Places.Add(4, 5, 6);

            IEnumerable<string> names = alpha.Names;
            IEnumerable<int> places = alpha.Places;

            Assert.That(names.Count(), Is.EqualTo(4));
            Assert.That(names.Aggregate("|", (a, b) => a + b + "|"), Is.EqualTo("|foo|bar|quad|quux|"));
            Assert.That(places.Count(), Is.EqualTo(3));
            Assert.That(places.Aggregate("|", (a, b) => a + b + "|"), Is.EqualTo("|4|5|6|"));
        }

        [Test]
        public void InvokeMemberWithSeveralArgumentsImpliesArrayInitialization() {
            dynamic alpha = new Clay(new PropBehavior(), new ArrayPropAssignmentBehavior());

            alpha
                .Names("foo", "bar", "quad")
                .Places(4, 5, 6);

            alpha.Names.Add("quux");

            IEnumerable<string> names = alpha.Names;
            IEnumerable<int> places = alpha.Places;

            Assert.That(names.Count(), Is.EqualTo(4));
            Assert.That(names.Aggregate("|", (a, b) => a + b + "|"), Is.EqualTo("|foo|bar|quad|quux|"));
            Assert.That(places.Count(), Is.EqualTo(3));
            Assert.That(places.Aggregate("|", (a, b) => a + b + "|"), Is.EqualTo("|4|5|6|"));
        }
    }
}
