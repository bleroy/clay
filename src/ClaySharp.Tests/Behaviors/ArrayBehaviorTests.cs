using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClaySharp.Behaviors;
using NUnit.Framework;

namespace ClaySharp.Tests.Behaviors {
    [TestFixture]
    public class ArrayBehaviorTests {
        [Test]
        public void AddGrowsArray() {
            dynamic array = new Clay(new ArrayBehavior());
            array.Add("Alpha");
            array.Add("Beta");

            Assert.That(array[0], Is.EqualTo("Alpha"));
            Assert.That(array[1], Is.EqualTo("Beta"));
        }


        [Test]
        public void LengthAndCountShowCurrentSize() {
            dynamic array = new Clay(new ArrayBehavior());

            Assert.That(array.Length, Is.EqualTo(0));
            Assert.That(array.Count, Is.EqualTo(0));
            Assert.That(array.Length(), Is.EqualTo(0));
            Assert.That(array.Count(), Is.EqualTo(0));

            array.Add("Alpha");
            array.Add("Beta");

            Assert.That(array.Length, Is.EqualTo(2));
            Assert.That(array.Count, Is.EqualTo(2));
            Assert.That(array.Length(), Is.EqualTo(2));
            Assert.That(array.Count(), Is.EqualTo(2));
        }

        [Test]
        public void AddCallsCanBeChained() {
            dynamic array = new Clay(new ArrayBehavior());
            array.Add("Alpha").Add("Beta");

            Assert.That(array[0], Is.EqualTo("Alpha"));
            Assert.That(array[1], Is.EqualTo("Beta"));
        }

        [Test]
        public void AddTakesZeroOrMoreArguments() {
            dynamic array = new Clay(new ArrayBehavior());
            array.Add().Add("Alpha").Add(null).Add("Beta", "Gamma", "Delta");

            Assert.That(array[0], Is.EqualTo("Alpha"));
            Assert.That((object)array[1], Is.Null);
            Assert.That(array[2], Is.EqualTo("Beta"));
            Assert.That(array[3], Is.EqualTo("Gamma"));
            Assert.That(array[4], Is.EqualTo("Delta"));
        }


        /*
         * Insert(i,t)#
         * IndexOf(t)#
         * RemoveAt(i)#
         * [i]#
         * 
         * Add(t)#
         * Clear()
         * Contains(t)#
         * CopyTo(arr, offs)
         * Remove(t)#
         * Count#
         * IsReadOnly
         * 
         * GetEnumerator
         * 
         * */

        [Test]
        public void InsertAndRemoveAtIndexes() {
            dynamic array = new Clay(new ArrayBehavior());
            array.Add("a", "b", "c", "d").Insert(2, "b++").RemoveAt(3);

            Assert.That(array.Count, Is.EqualTo(4));
            Assert.That(array[0], Is.EqualTo("a"));
            Assert.That(array[1], Is.EqualTo("b"));
            Assert.That(array[2], Is.EqualTo("b++"));
            Assert.That(array[3], Is.EqualTo("d"));

        }

        [Test]
        public void InsertMayTakeSeveral() {
            dynamic array = new Clay(new ArrayBehavior());
            array.Add("a", "b", "c", "d").Insert(2, "b2", "b3", "b4").RemoveAt(3);

            Assert.That(array.Count, Is.EqualTo(6));
            Assert.That(array[0], Is.EqualTo("a"));
            Assert.That(array[1], Is.EqualTo("b"));
            Assert.That(array[2], Is.EqualTo("b2"));
            Assert.That(array[3], Is.EqualTo("b4"));
            Assert.That(array[4], Is.EqualTo("c"));
            Assert.That(array[5], Is.EqualTo("d"));
        }


        [Test]
        public void ContainsRemoveAndIndexOfFunctionAsNormalListWouldDictate() {
            dynamic array = new Clay(new ArrayBehavior());
            array.Add("a", "b", "c", "d");

            Assert.That(array.Contains("b"), Is.True);
            Assert.That(array.Contains("e"), Is.False);
            Assert.That(array.IndexOf("b"), Is.EqualTo(1));
            Assert.That(array.IndexOf("e"), Is.EqualTo(-1));
            Assert.That(array.Remove("b"), Is.True);
            Assert.That(array.Remove("e"), Is.False);

            Assert.That(array.Contains("b"), Is.False);
            Assert.That(array.IndexOf("b"), Is.EqualTo(-1));
            Assert.That(array.Remove("b"), Is.False);
        }

        [Test]
        public void IteratingListReturnsValues() {
            dynamic array = new Clay(new ArrayBehavior(), new InterfaceProxyBehavior());
            array.Add("a", "b", "c", "d");

            var expectedCharacters = "abcd".GetEnumerator();

            foreach (var item in array) {
                Assert.That(expectedCharacters.MoveNext(), Is.True);
                Assert.That(item, Is.EqualTo(expectedCharacters.Current.ToString()));
            }
            Assert.That(expectedCharacters.MoveNext(), Is.False);
        }

        [Test]
        public void CallingGetEnumeratorDirectlyOnDynamic() {
            dynamic array = new Clay(new ArrayBehavior(), new InterfaceProxyBehavior());
            array.Add("hello");

            IEnumerator enum1 = array.GetEnumerator();
            Assert.That(enum1.MoveNext(), Is.True);
            Assert.That(enum1.Current, Is.EqualTo("hello"));
            Assert.That(enum1.MoveNext(), Is.False);

            IEnumerator<object> enum2 = array.GetEnumerator();
            Assert.That(enum2.MoveNext(), Is.True);
            Assert.That(enum2.Current, Is.EqualTo("hello"));
            Assert.That(enum2.MoveNext(), Is.False);
        }




        /*
         * Insert(i,t)#
         * IndexOf(t)#
         * RemoveAt(i)#
         * [i]#
         * 
         * Add(t)#
         * Clear()
         * Contains(t)#
         * CopyTo(arr, offs)
         * Remove(t)#
         * Count#
         * IsReadOnly
         * 
         * GetEnumerator
         * 
         * */

        [Test]
        public void UsingArrayWithVarietyOfCollectionInterfaces() {
            dynamic array = new Clay(new InterfaceProxyBehavior(), new ArrayBehavior());

            array.Add("a", "b", "c", "d");

            ICollection<string> collectionString = array;

            Assert.That(collectionString.Contains("e"), Is.False);
            collectionString.Add("e");
            Assert.That(collectionString.Contains("e"), Is.True);
            Assert.That(collectionString.Count, Is.EqualTo(5));

            Assert.That(collectionString.Count(), Is.EqualTo(5));


            IList<string> listString = array;
            Assert.That(listString.IndexOf("b++"), Is.EqualTo(-1));
            Assert.That(listString.IndexOf("c"), Is.EqualTo(2));
            listString.Insert(2, "b++");
            Assert.That(listString.IndexOf("b++"), Is.EqualTo(2));
            Assert.That(listString.IndexOf("c"), Is.EqualTo(3));
            listString.RemoveAt(2);
            Assert.That(listString.IndexOf("b++"), Is.EqualTo(-1));
            Assert.That(listString.IndexOf("c"), Is.EqualTo(2));

            Assert.That(listString[1], Is.EqualTo("b"));
            Assert.That(listString[2], Is.EqualTo("c"));
        }

        [Test]
        public void UsingViaSystemInterfacesWithLinqExtensionMethods() {
            dynamic array = new Clay(new InterfaceProxyBehavior(), new ArrayBehavior());

            array.Add("a", "b", "c", "d", "e");

            IEnumerable enumerableBase = array;
            ICollection collectionBase = array;
            IList listBase = array;

            IEnumerable<object> enumerableObject = array;
            ICollection<object> collectionObject = array;
            IList<object> listObject = array;

            IEnumerable<string> enumerableString = array;
            ICollection<string> collectionString = array;
            IList<string> listString = array;

            Assert.That(enumerableBase.Cast<object>().Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));
            Assert.That(collectionBase.Cast<object>().Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));
            Assert.That(listBase.Cast<object>().Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));

            Assert.That(enumerableBase.OfType<object>().Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));
            Assert.That(collectionBase.OfType<object>().Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));
            Assert.That(listBase.OfType<object>().Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));

            Assert.That(enumerableBase.Cast<string>().Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));
            Assert.That(collectionBase.Cast<string>().Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));
            Assert.That(listBase.Cast<string>().Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));

            Assert.That(enumerableBase.OfType<string>().Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));
            Assert.That(collectionBase.OfType<string>().Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));
            Assert.That(listBase.OfType<string>().Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));

            Assert.That(enumerableObject.Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));
            Assert.That(collectionObject.Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));
            Assert.That(listObject.Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));

            Assert.That(enumerableString.Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));
            Assert.That(collectionString.Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));
            Assert.That(listString.Reverse().Skip(1).Take(3).Last(), Is.EqualTo("b"));

            var enumerator = enumerableString.GetEnumerator();
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo("a"));


            enumerator = collectionString.GetEnumerator();
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo("a"));

            enumerator = listString.GetEnumerator();
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo("a"));
        }

        [Test]
        public void ArrayCombinesWithOtherBehaviors() {
            dynamic combo = new Clay(
                new InterfaceProxyBehavior(),
                new PropBehavior(),
                new ArrayBehavior(),
                new NilResultBehavior());

            Assert.That(combo.Count, Is.EqualTo(0));

            combo.Hello = "world";
            Assert.That(combo.Hello, Is.EqualTo("world"));
            Assert.That(combo.Hello(), Is.EqualTo("world"));
            Assert.That(combo["Hello"], Is.EqualTo("world"));

            Assert.That(combo.Count, Is.EqualTo(0));

            combo.Add("alpha", "beta");
            Assert.That(combo.Count, Is.EqualTo(2));
            Assert.That(combo[0], Is.EqualTo("alpha"));
            Assert.That(combo[1], Is.EqualTo("beta"));

            var c2 = (ICombo)combo;
            Assert.That(c2.Hello, Is.EqualTo("world"));
            Assert.That(c2.Length, Is.EqualTo(2));
            Assert.That(c2.Count, Is.EqualTo(2));
            Assert.That(c2[0], Is.EqualTo("alpha"));
            Assert.That(c2[1], Is.EqualTo("beta"));
            Assert.That(c2.Again, Is.Null);

            Assert.That(c2.Extra.Title, Is.Null);
            Assert.That(c2.Extra.Description, Is.Null);

            Assert.That(c2.Aggregate(">", (a, b) => a + "(" + b + ")"), Is.EqualTo(">(alpha)(beta)"));
        }

        public interface ICombo : ICollection<string> {
            string Hello { get; set; }
            string Again { get; set; }
            int Length { get; }
            object this[int index] { get; set; }
            ISafeNilResult Extra { get; set; }
        }

        public interface ISafeNilResult {
            string Title { get; set; }
            string Description { get; set; }
        }
    }
}
