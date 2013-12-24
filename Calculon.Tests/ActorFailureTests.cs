/* ----------------------------------------------------------------------------
 * Copyright (C) 2013 Arne F. Claassen
 * geekblog [at] claassen [dot] net
 * http://github.com/sdether/Calculon
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 * ----------------------------------------------------------------------------
 */

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