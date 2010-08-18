using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClaySharp.Implementation;
using NUnit.Framework;

namespace ClaySharp.Tests.Implementation {
    [TestFixture]
    public class NamedArgumentsTests {

        private INamedEnumerable<int> AllNamed() {
            return Arguments.FromT(new[] { 1, 2, 3 }, new[] { "a", "b", "c" });
        }

        private INamedEnumerable<int> AllPositional() {
            return Arguments.FromT(new[] { 1, 2, 3 }, Enumerable.Empty<string>());
        }

        [Test]
        public void ZeroNamesGivesYouEntirelyPositionalArguments() {
            var args = AllPositional();
            Assert.That(args.Count(), Is.EqualTo(3));
            Assert.That(args.Positional.Count(), Is.EqualTo(3));
            Assert.That(args.Named.Count(), Is.EqualTo(0));
        }
        [Test]
        public void EqualNamesGivesYouEntirelyNamedArguments() {
            var args = AllNamed();
            Assert.That(args.Count(), Is.EqualTo(3));
            Assert.That(args.Positional.Count(), Is.EqualTo(0));
            Assert.That(args.Named.Count(), Is.EqualTo(3));
        }

        [Test]
        public void IteratingEmptyNamedAndPositionalCollections() {
            foreach (var named in AllPositional().Named) {
                Assert.Fail("Should not have named items");
            }

            foreach (var positional in AllNamed().Positional) {
                Assert.Fail("Should not have positional items");
            }
        }

        [Test]
        public void ContainsKeyWorksOnNames() {
            var args = AllNamed();
            Assert.That(args.Named.ContainsKey("a"), Is.True);
            Assert.That(args.Named.ContainsKey("d"), Is.False);

            var args2 = AllPositional();
            Assert.That(args2.Named.ContainsKey("a"), Is.False);
            Assert.That(args2.Named.ContainsKey("d"), Is.False);
        }

        [Test]
        public void ContainsWorksOnNameArgumentPairs() {
            var args = AllNamed();
            Assert.That(args.Named.Contains(new KeyValuePair<string, int>("a", 1)), Is.True);
            Assert.That(args.Named.Contains(new KeyValuePair<string, int>("a", 2)), Is.False);
            Assert.That(args.Named.Contains(new KeyValuePair<string, int>("d", 0)), Is.False);
        }


        [Test]
        public void NamedKeysCollectionIsAvailable() {
            var args = AllNamed();
            Assert.That(args.Named.Keys.Count(), Is.EqualTo(3));
            Assert.That(args.Named.Keys.Aggregate(">", (a, b) => a + b), Is.EqualTo(">abc"));

            var args2 = AllPositional();
            Assert.That(args2.Named.Keys.Count(), Is.EqualTo(0));
            Assert.That(args2.Named.Keys.Aggregate(">", (a, b) => a + b), Is.EqualTo(">"));
        }

        [Test]
        public void NamedValueCollectionIsAvailable() {
            var args = AllNamed();
            Assert.That(args.Named.Values.Count(), Is.EqualTo(3));
            Assert.That(args.Named.Values.Aggregate(">", (a, b) => a + b), Is.EqualTo(">123"));

            var args2 = AllPositional();
            Assert.That(args2.Named.Values.Count(), Is.EqualTo(0));
            Assert.That(args2.Named.Values.Aggregate(">", (a, b) => a + b), Is.EqualTo(">"));
        }

        [Test]
        public void NamedCollectionIsAvailable() {
            var args = AllNamed();
            Assert.That(args.Named.Count(), Is.EqualTo(3));
            Assert.That(args.Named.Aggregate(">", (a, b) => a + b.Key + b.Value), Is.EqualTo(">a1b2c3"));

            var args2 = AllPositional();
            Assert.That(args2.Named.Count(), Is.EqualTo(0));
            Assert.That(args2.Named.Aggregate(">", (a, b) => a + b.Key + b.Value), Is.EqualTo(">"));
        }

        [Test]
        public void NameIndexerWorksAsExpectedWithDefaultValueOnKeyMiss() {
            var args = AllNamed();
            Assert.That(args.Named["a"], Is.EqualTo(1));
            Assert.That(args.Named["b"], Is.EqualTo(2));
            Assert.That(args.Named["c"], Is.EqualTo(3));
            Assert.That(args.Named["d"], Is.EqualTo(default(int)));

            int vala;
            var hasa = args.Named.TryGetValue("a", out vala);
            Assert.That(vala, Is.EqualTo(1));
            Assert.That(hasa, Is.True);

            int vald;
            var hasd = args.Named.TryGetValue("d", out vald);
            Assert.That(vald, Is.EqualTo(default(int)));
            Assert.That(hasd, Is.False);
        }
    }
}
