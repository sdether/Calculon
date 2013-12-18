using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Droog.Calculon.Tests {

    [TestFixture]
    public class StageTests {

        public interface IAdder {
            Task<int> Add(int a, int b);
        }

        public class Adder : AActor,IAdder {
            public Task<int> Add(int a, int b) {
                return Scene.Return(a + b);
            }
        }

        [Test]
        public void Can_create_and_call_actor() {
            var stage = new Stage(new ActorBuilder());
            var adder = stage.Create<IAdder>();
            var t = adder.Add(1, 1);
            t.Wait();
            Assert.AreEqual(2,t.Result);
        }
    }
}
