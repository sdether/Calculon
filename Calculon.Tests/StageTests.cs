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
using System.Diagnostics;
using System.Threading.Tasks;
using Droog.Calculon.Tests.Actors;
using NUnit.Framework;

namespace Droog.Calculon.Tests {
    [TestFixture]
    public class StageTests {

        [Test]
        public void Can_create_and_Ask() {
            var stage = new Stage();
            var echo = stage.Create<IEcho>().Proxy;
            var r1 = echo.Ask("foo").WaitForResult();
            var r2 = echo.Ask("bar").WaitForResult();
            Assert.AreEqual("foo", r1);
            Assert.AreEqual("bar", r2);
        }

        [Test]
        public void Can_create_and_Notify() {
            var stage = new Stage();
            var echo = stage.Create<IEcho>().Proxy;
            var t = echo.Notify();
            t.Wait();
            Assert.IsTrue(t.IsCompleted);
        }

        [Test]
        public void Can_create_and_Tell() {
            var stage = new Stage();
            var echo = stage.Create<IEcho>().Proxy;
            echo.Tell();
            var verify = echo.VerifyTell().WaitForResult();
            Assert.IsTrue(verify);
        }

        [Test]
        public void Can_chain_actor_calls() {
            var stage = new Stage();
            var adder = stage.Create<IAdder>().Proxy;
            var t = adder.Add(1, 1)
                .ContinueWith(t1 => adder.Add(t1.Result, 2));
            t.Wait();
            Assert.AreEqual(4, t.Unwrap().Result);
        }

        [Test]
        public void Can_call_delayed_call() {
            var stage = new Stage();
            var adder = stage.Create<IAdder>().Proxy;
            var t1 = adder.AddDelayed(1, 1, TimeSpan.FromSeconds(1));
            var sw = Stopwatch.StartNew();
            t1.Wait();
            sw.Stop();
            Assert.AreEqual(2, t1.Result);
            Assert.GreaterOrEqual(sw.ElapsedMilliseconds, 900);
        }

        [Test]
        public void Can_find_actor_by_ref() {
            var stage = new Stage();
            var a1 = stage.Create<IIdentity>();
            var b1 = stage.Create<IIdentity>();
            var a2 = stage.Find<IIdentity>(a1.Ref);
            var b2 = stage.Find<IIdentity>(b1.Ref);
            var a1Id = a1.Proxy.GetIdentity().WaitForResult();
            var a2Id = a2.Proxy.GetIdentity().WaitForResult();
            var b1Id = b1.Proxy.GetIdentity().WaitForResult();
            var b2Id = b2.Proxy.GetIdentity().WaitForResult();

            Assert.AreNotEqual(a1Id, b1Id, "actor ids were the same");
            Assert.AreEqual(a1Id, a2Id, "a ids did not match");
            Assert.AreEqual(b1Id, b2Id, "b ids did not match");
        }
    }
}
