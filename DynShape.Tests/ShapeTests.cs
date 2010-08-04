using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DynShape.Behaviors;
using NUnit.Framework;

namespace DynShape.Tests {


    [TestFixture]
    public class ShapeTests {
        public ShapeHelper S { get; set; }
        public dynamic F { get; set; }

        [SetUp]
        public void Init() {
            S = new ShapeHelper();
            F = new Thing(new ShapeFactoryBehavior());
        }

        IEnumerable<T> Cat<T>(T t) {
            yield return t;
        }

        [Test]
        public void PropertiesCanBeAssignedAndRetrieved() {
            dynamic shape = new Thing(Cat(new PropBehavior()));
            var foo = "bar";
            shape.Foo = "textbox";
            Assert.That((object)shape.Foo, Is.EqualTo("textbox"));
        }

        interface ITest {
            string Foo { get; set; }
        }

        [Test]
        public void TypeCastToInterface() {
            dynamic shape = new Thing(Cat(new PropBehavior()));
            var test0 = shape is ITest;
            var test1 = shape as ITest;
            var test2 = (ITest)shape;
            ITest test3 = shape;
            Assert.That(test0, Is.True);
            Assert.That(test1, Is.Not.Null);
            Assert.That(test2, Is.Not.Null);
            Assert.That(test3, Is.Not.Null);
        }


        [Test]
        public void CreateSyntax() {

            var form = F.Form(new { Misc = 4 })
                .Actions(F.Fieldset()
                    .Save(F.Foo().Value("Save").Id("Hello"))
                    .Cancel(F.Foo().Value("Cancel")));


            var foo = F.Foo(new { Value = "Save", Id = "Hello" });

            var bar = F.Foo(new { Bleah = (object)null});
            
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

            var foo8 = bar.Foo ? bar.Foo : (dynamic)"yarg";


            Assert.That(foo1, Is.SameAs(Nil.Instance));
            Assert.That(foo2, Is.Null);
            Assert.That(foo3, Is.Null);
            Assert.That(foo4, Is.True);
            Assert.That(foo5, Is.False);
            Assert.That(foo6, Is.Null);
            Assert.That(foo7, Is.EqualTo("yarg"));
            Assert.That(foo8, Is.EqualTo("yarg"));

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
        }

        [Test]
        public void ShapeFactorySetsShapeName() {
            var x1 = F.Something();
            var x2 = F.SomethingElse();

            Assert.That(x1.ShapeName, Is.EqualTo("Something"));
            Assert.That(x2.ShapeName, Is.EqualTo("SomethingElse"));
        }

        [Test]
        public void OptionsArgumentSetsProperties() {
            var x = F.Something(new { One = "1", Two = 2 });

            Assert.That(x.ShapeName, Is.EqualTo("Something"));
            Assert.That(x.One, Is.EqualTo("1"));
            Assert.That(x.Two, Is.EqualTo(2));
        }
    }

    public class ShapeHelper {
        public dynamic New(string shapeName, Action<dynamic> initialize) {
            var item = new Thing(new PropBehavior());
            initialize(item);
            return item;
        }
        public dynamic New(string shapeName) {
            return New(shapeName, item => { });
        }
    }

    public static class ShapeHelperExtensions {
        public static dynamic TextBox(this ShapeHelper shapeHelper, Action<dynamic> initialize) {
            return shapeHelper.New("textbox", initialize);
        }
    }
}
