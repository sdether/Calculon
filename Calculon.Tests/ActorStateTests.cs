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