using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Droog.Calculon.Tests {

    [TestFixture]
    public class StageTests {

        public interface IAdder {
            Task<int> Add(int a, int b);
            Task<int> AddDelayed(int a, int b, TimeSpan delay);
        }

        public class Adder : AActor, IAdder {
            public Task<int> Add(int a, int b) {
                return Scene.Return(a + b);
            }

            public Task<int> AddDelayed(int a, int b, TimeSpan delay) {
                var completion = Scene.GetCompletion<int>();
                new Timer(_ => completion.SetResult(a + b)).Change(delay, TimeSpan.Zero);
                return completion;
            }
        }

        [Test]
        public void Can_create_and_call_actor() {
            var stage = new Stage();
            var adder = stage.CreateAndGet<IAdder>();
            var t1 = adder.Add(1, 1);
            var t2 = adder.Add(2, 2);
            t1.Wait();
            t2.Wait();
            Assert.AreEqual(2, t1.Result);
            Assert.AreEqual(4, t2.Result);
        }

        [Test]
        public void Can_chain_actor_calls() {
            var stage = new Stage();
            var adder = stage.CreateAndGet<IAdder>();
            var t = adder.Add(1, 1)
                .ContinueWith(t1 => adder.Add(t1.Result, 2));
            t.Wait();
            Assert.AreEqual(4, t.Unwrap().Result);
        }

        [Test]
        public void Can_call_delayed_call() {
            var stage = new Stage();
            var adder = stage.CreateAndGet<IAdder>();
            var t1 = adder.AddDelayed(1, 1, TimeSpan.FromSeconds(1));
            var sw = Stopwatch.StartNew();
            t1.Wait();
            sw.Stop();
            Assert.AreEqual(2, t1.Result);
            Assert.GreaterOrEqual(sw.ElapsedMilliseconds, 900);
        }
    }
}
