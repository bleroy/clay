using System;
using System.Collections.Generic;
using ClaySharp.Behaviors;
using NUnit.Framework;

namespace ClaySharp.Tests {


    [TestFixture]
    public class ClayTests {
        public ClayHelper S { get; set; }
        public dynamic New { get; set; }

        [SetUp]
        public void Init() {
            S = new ClayHelper();
            New = new ClayFactory();
        }

        static IEnumerable<T> Cat<T>(T t) {
            yield return t;
        }

        [Test]
        public void PropertiesCanBeAssignedAndRetrieved() {
            dynamic shape = new Clay(Cat(new PropBehavior()));
            shape.Foo = "textbox";
            Assert.That((object)shape.Foo, Is.EqualTo("textbox"));
        }

        public interface ITest {
            string Foo { get; set; }
            ITestSub Bar { get; set; }
        }

        public interface ITestSub {
            string FooSub { get; set; }
        }


        [Test]
        public void TypeCastToInterface() {
            dynamic shape = new Clay(new PropBehavior(), new NilResultBehavior(), new InterfaceProxyBehavior());
            dynamic shape2 = new Clay(new PropBehavior(), new NilResultBehavior(), new InterfaceProxyBehavior());

            var test0 = shape is ITest;
            var test1 = shape as ITest;
            var test2 = (ITest)shape;
            ITest test3 = shape;
            Assert.That(test0, Is.False);
            Assert.That(test1, Is.Null);
            Assert.That(test2, Is.Not.Null);
            Assert.That(test3, Is.Not.Null);

            Assert.That((string)shape.Foo, Is.Null);
            Assert.That(test2.Foo, Is.Null);
            shape.Foo = "Bar";
            Assert.That(shape.Foo, Is.EqualTo("Bar"));
            Assert.That(test2.Foo, Is.EqualTo("Bar"));
            test2.Foo = "Quux";
            Assert.That(shape.Foo, Is.EqualTo("Quux"));
            Assert.That(test2.Foo, Is.EqualTo("Quux"));

            Assert.That(test2.Bar, Is.Not.Null);
            Assert.That(shape.Bar, Is.SameAs(Nil.Instance));
            Assert.That(test2.Bar.FooSub, Is.Null);
            Assert.That(shape.Bar.FooSub, Is.SameAs(Nil.Instance));

            test2.Bar = shape2;

            Assert.That(test2.Bar, Is.Not.Null);
            Assert.That(shape.Bar, Is.Not.Null);

            Assert.That(test2.Bar.FooSub, Is.Null);
            Assert.That(shape.Bar.FooSub, Is.SameAs(Nil.Instance));

            shape2.FooSub = "Yarg";
            Assert.That(test2.Bar.FooSub, Is.EqualTo("Yarg"));
            Assert.That(shape.Bar.FooSub, Is.EqualTo("Yarg"));

        }


        public interface ITestForm {
            string ShapeName();
            object this[object key] { get; set; }

            ITestActions Actions { get; set; }
            int? Misc { get; set; }
        }

        public interface ITestActions {
            string ShapeName { get; set; }
            IButton Save { get; set; }
            IButton Cancel { get; set; }
            IButton Preview { get; set; }
        }

        public interface IButton {
            string ShapeName { get; set; }
            string Id { get; set; }
            string Value { get; set; }
        }

        [Test]
        public void CreateSyntax() {

            var form = New.Form(new { Misc = 4 })
                .Actions(New.Fieldset()
                    .Save(New.Button().Value("Save").Id("Hello"))
                    .Cancel(New.Button().Value("Cancel")));


            var bar = New.Foo(new { Bleah = (object)null });

            Assert.That(bar.Bleah(), Is.SameAs(Nil.Instance));
            Assert.That(bar.Bleah, Is.SameAs(Nil.Instance));
            Assert.That(bar.Yarg, Is.SameAs(Nil.Instance));
            Assert.That(bar.One.Two.Three()["Four"], Is.SameAs(Nil.Instance));

            var foo1 = bar.Foo;
            string foo2 = bar.Foo;
            int? foo3 = bar.Foo;
            var foo4 = bar.Foo == null;
            var foo5 = bar.Foo != null;
            var foo6 = (ITest)bar.Foo;
            var foo7 = (string)bar.Foo ?? "yarg";

            //            var foo8 = bar.Foo ? bar.Foo : (dynamic)"yarg";


            Assert.That(foo1, Is.SameAs(Nil.Instance));
            Assert.That(foo2, Is.Null);
            Assert.That(foo3, Is.Null);
            Assert.That(foo4, Is.True);
            Assert.That(foo5, Is.False);
            Assert.That(foo6, Is.Not.Null);
            Assert.That(foo7, Is.EqualTo("yarg"));
            //Assert.That(foo8, Is.EqualTo("yarg"));

            //            form.Actions += bar;

            form.Misc += 3;

            Assert.That(form.ShapeName, Is.EqualTo("Form"));
            Assert.That(form.Misc, Is.EqualTo(7));
            Assert.That(form.Actions.Save.Id, Is.EqualTo("Hello"));
            Assert.That(form.Actions.Save.Value, Is.EqualTo("Save"));
            Assert.That(form.Actions.Cancel.Value, Is.EqualTo("Cancel"));

            Assert.That(form.Misc(), Is.EqualTo(7));
            Assert.That(form.Actions().Save().Id(), Is.EqualTo("Hello"));
            Assert.That(form.Actions().Save().Value(), Is.EqualTo("Save"));
            Assert.That(form.Actions().Cancel().Value(), Is.EqualTo("Cancel"));

            form[3] = "hello";

            Assert.That(form["Misc"], Is.EqualTo(7));
            Assert.That(form["Actions"]["Save"]["Id"], Is.EqualTo("Hello"));
            Assert.That(form["Actions"]["Save"]["Value"], Is.EqualTo("Save"));
            Assert.That(form["Actions"]["Cancel"]["Value"], Is.EqualTo("Cancel"));

            ITestForm f = form;
            Assert.That(f.Misc, Is.EqualTo(7));
            Assert.That(f.Actions.ShapeName, Is.EqualTo("Fieldset"));
            Assert.That(f.Actions.Save.Id, Is.EqualTo("Hello"));
            Assert.That(f.Actions.Save.Value, Is.EqualTo("Save"));
            Assert.That(f.Actions.Cancel.Value, Is.EqualTo("Cancel"));
            Assert.That(f.Actions.Preview.Id, Is.Null);
            Assert.That((dynamic)f.Actions.Preview == null);

            Assert.That(f["Misc"], Is.EqualTo(7));
            f["Misc"] = 4;
            Assert.That(f.Misc, Is.EqualTo(4));
            Assert.That(f["Misc"], Is.EqualTo(4));
            Assert.That(form.Misc, Is.EqualTo(4));

            f.Misc = 9;
            Assert.That(f.Misc, Is.EqualTo(9));
            Assert.That(f["Misc"], Is.EqualTo(9));
            Assert.That(form.Misc, Is.EqualTo(9));
        }

        [Test]
        public void CreateArraySyntax() {
            var directory = New.Array(
                New.Person().Name("Louis").Aliases(new [] {"Lou"}),
                New.Person().Name("Bertrand").Aliases("bleroy", "boudin")
                ).Name("Orchard folks");

            Assert.That(directory.Count, Is.EqualTo(2));
            Assert.That(directory.Name, Is.EqualTo("Orchard folks"));
            Assert.That(directory[0].Name, Is.EqualTo("Louis"));
            Assert.That(directory[0].Aliases.Count, Is.EqualTo(1));
            Assert.That(directory[0].Aliases[0], Is.EqualTo("Lou"));
            Assert.That(directory[1].Name, Is.EqualTo("Bertrand"));
            Assert.That(directory[1].Aliases.Count, Is.EqualTo(2));
            Assert.That(directory[1].Aliases[0], Is.EqualTo("bleroy"));
            Assert.That(directory[1].Aliases[1], Is.EqualTo("boudin"));
        }

        public interface IPerson {
            string FirstName { get; set; }
            string LastName { get; set; }
        }

        [Test]
        public void BertrandsAssumptions() {
            var pentagon = New.Shape();
            pentagon["FavoriteNumber"] = 5;

            Assert.That(pentagon.FavoriteNumber, Is.EqualTo(5));
            Assert.That(pentagon.SomethingNeverSet, Is.SameAs(Nil.Instance));

            var person = New.Person()
                .FirstName("Louis")
                .LastName("Dejardin");
            Assert.That(person.FirstName, Is.EqualTo("Louis"));
            Assert.That(person["FirstName"], Is.EqualTo("Louis"));
            Assert.That(person.FirstName(), Is.EqualTo("Louis"));
            Assert.That(person.LastName, Is.EqualTo("Dejardin"));

            var otherPerson = New.Person(new {
                FirstName = "Bertrand",
                LastName = "Le Roy"
                });
            Assert.That(otherPerson.FirstName, Is.EqualTo("Bertrand"));
            Assert.That(otherPerson.LastName, Is.EqualTo("Le Roy"));

            var yetAnotherPerson = New.Person(
                FirstName: "Renaud",
                LastName: "Paquay"
                );
            Assert.That(yetAnotherPerson.FirstName, Is.EqualTo("Renaud"));
            Assert.That(yetAnotherPerson.LastName, Is.EqualTo("Paquay"));

            var people = New.Array(
                New.Person().FirstName("Louis").LastName("Dejardin"),
                New.Person().FirstName("Bertrand").LastName("Le Roy")
                );
            Assert.That(people.Count, Is.EqualTo(2));
            Assert.That(people[0].FirstName, Is.EqualTo("Louis"));
            Assert.That(people[1].FirstName, Is.EqualTo("Bertrand"));
            Assert.That(people[0].LastName, Is.EqualTo("Dejardin"));
            Assert.That(people[1].LastName, Is.EqualTo("Le Roy"));

            var a = "";
            foreach(var p in people) {
                a += p.FirstName + "|";
            }
            Assert.That(a, Is.EqualTo("Louis|Bertrand|"));

            otherPerson.Aliases("bleroy", "BoudinFatal");
            Assert.That(otherPerson.Aliases.Count, Is.EqualTo(2));

            person.Aliases(new[] {"Lou"});
            Assert.That(person.Aliases.Count, Is.EqualTo(1));
            person.Aliases.Add("loudej");
            Assert.That(person.Aliases.Count, Is.EqualTo(2));

            IPerson lou = people[0];
            var fullName = lou.FirstName + " " + lou.LastName;
            Assert.That(fullName, Is.EqualTo("Louis Dejardin"));
        }

        [Test]
        public void ShapeFactorySetsShapeName() {
            var x1 = New.Something();
            var x2 = New.SomethingElse();

            Assert.That(x1.ShapeName, Is.EqualTo("Something"));
            Assert.That(x2.ShapeName, Is.EqualTo("SomethingElse"));
        }

        [Test]
        public void OptionsArgumentSetsProperties() {
            var x = New.Something(new { One = "1", Two = 2 });

            Assert.That(x.ShapeName, Is.EqualTo("Something"));
            Assert.That(x.One, Is.EqualTo("1"));
            Assert.That(x.Two, Is.EqualTo(2));
        }

    }

    public class ClayHelper {
        public dynamic New(string shapeName, Action<dynamic> initialize) {
            var item = new Clay(new PropBehavior());
            initialize(item);
            return item;
        }
        public dynamic New(string shapeName) {
            return New(shapeName, item => { });
        }
    }



    public static class ClayHelperExtensions {
        public static dynamic TextBox(this ClayHelper clayHelper, Action<dynamic> initialize) {
            return clayHelper.New("textbox", initialize);
        }
    }
}
