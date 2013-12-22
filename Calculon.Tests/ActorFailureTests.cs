using System;
using System.Threading.Tasks;
using Droog.Calculon.Tests.Actors;
using NUnit.Framework;

namespace Droog.Calculon.Tests {
    [TestFixture]
    public class ActorFailureTests {

        [Test]
        public void Actor_can_signal_failure_via_task() {
            var stage = new Stage();
            var a = stage.Create<IFailing>().Proxy;
            var t = a.FailImmediate();
            Assert.IsTrue(Wait(t), "task never completed");
            Assert.IsTrue(t.IsFaulted, "task isn't faulted");
            Assert.IsInstanceOfType(typeof(Failure), t.Exception.GetBaseException());
        }

        [Test]
        public void Actor_can_signal_failure_via_task_asynchronously() {
            var stage = new Stage();
            var a = stage.Create<IFailing>().Proxy;
            var t = a.FailLater();
            Assert.IsTrue(Wait(t), "task never completed");
            Assert.IsTrue(t.IsFaulted, "task isn't faulted");
            Assert.IsInstanceOfType(typeof(Failure), t.Exception.GetBaseException());
        }

        [Test]
        public void Actor_can_signal_failure_by_throwing() {
            var stage = new Stage();
            var a = stage.Create<IFailing>().Proxy;
            var t = a.FailByThrowing();
            Assert.IsTrue(Wait(t), "task never completed");
            Assert.IsTrue(t.IsFaulted, "task isn't faulted");
            Assert.IsInstanceOfType(typeof(Failure), t.Exception.GetBaseException());
        }
        [Test]
        public void Actor_can_signal_failure_via_task_of_t() {
            var stage = new Stage();
            var a = stage.Create<IFailing>().Proxy;
            var t = a.FailImmediateOfT();
            Assert.IsTrue(Wait(t), "task never completed");
            Assert.IsTrue(t.IsFaulted, "task isn't faulted");
            Assert.IsInstanceOfType(typeof(Failure), t.Exception.GetBaseException());
        }

        [Test]
        public void Actor_can_signal_failure_via_task_of_t_asynchronously() {
            var stage = new Stage();
            var a = stage.Create<IFailing>().Proxy;
            var t = a.FailLaterOfT();
            Assert.IsTrue(Wait(t), "task never completed");
            Assert.IsTrue(t.IsFaulted, "task isn't faulted");
            Assert.IsInstanceOfType(typeof(Failure), t.Exception.GetBaseException());
        }

        [Test]
        public void Actor_can_signal_failure_via_task_of_t_by_throwing() {
            var stage = new Stage();
            var a = stage.Create<IFailing>().Proxy;
            var t = a.FailByThrowingOfT();
            Assert.IsTrue(Wait(t), "task never completed");
            Assert.IsTrue(t.IsFaulted, "task isn't faulted");
            Assert.IsInstanceOfType(typeof(Failure), t.Exception.GetBaseException());
        }

        [Test]
        public void Exception_in_void_method_does_not_disable_actor() {
            var stage = new Stage();
            var a = stage.Create<IFailing>().Proxy;
            a.VoidFailure();
            a.VoidFailure();
            var count = a.VoidFailureCalls().WaitForResult();
            Assert.AreEqual(2, count);
        }

        private bool Wait(Task t) {
            try {
                return t.Wait(TimeSpan.FromSeconds(5));
            } catch(AggregateException e) {
                return true;
            }
        }

    }
}