using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Droog.Calculon.Tests.Actors;
using NUnit.Framework;

namespace Droog.Calculon.Tests {
    [TestFixture]
    public class ActorStateTests {

        [Test]
        public void Actors_do_not_share_state() {
            var stage = new Stage();
            var a1 = stage.Create<ICounter>().Proxy;
            var a2 = stage.Create<ICounter>().Proxy;

            var a1Tasks = Enumerable.Range(0, 10).Select(x => a1.Increment()).ToArray();
            var a2Tasks = Enumerable.Range(0, 5).Select(x => a2.Increment()).ToArray();
            Task.WaitAll(a1Tasks);
            Console.WriteLine("a1 increments done");
            Task.WaitAll(a2Tasks);
            Console.WriteLine("a2 increments done");
            var a1Current = a1.Current();
            var a2Current = a2.Current();
            a1Current.Wait();
            Console.WriteLine("a1 current done");
            a2Current.Wait();
            Console.WriteLine("a2 current done");

            Assert.AreEqual(10, a1Current.Result);
            Assert.AreEqual(5, a2Current.Result);
        }
    }
}