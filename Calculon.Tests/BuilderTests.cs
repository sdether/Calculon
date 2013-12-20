using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Droog.Calculon.Tests {
    
    [TestFixture]
    public class BuilderTests {

        public interface IFoo {
             
        }
        public class Foo : AActor, IFoo {
            
        }

        [Test]
        public void Can_create_class() {

            var builder = new ActorBuilder();
            var foo = builder.GetBuilder<Foo>()();
            Assert.IsNotNull(foo);
            Assert.IsInstanceOfType(typeof(Foo),foo);
        }

        [Test]
        public void Can_create_interface_with_appropriately_named_class() {

            var builder = new ActorBuilder();
            var foo = builder.GetBuilder<IFoo>()();
            Assert.IsNotNull(foo);
            Assert.IsInstanceOfType(typeof(Foo), foo);
        }
    }
}
