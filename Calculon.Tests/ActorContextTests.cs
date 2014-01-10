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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Droog.Calculon.Tests.Actors;
using NUnit.Framework;

namespace Droog.Calculon.Tests {
    [TestFixture]
    public class ActorContextTests {
        public interface IContextTester : IActor {
            Task<Guid[]> Can_find_actor_by_ref();
            Task<Tuple<Guid, ActorRef>> Can_create_child_actor();
        }

        public class ContextTester : AActor, IContextTester {

            public Task<Guid[]> Can_find_actor_by_ref() {
                var a1 = Context.Create<IIdentity>("id1");
                var b1 = Context.Create<IIdentity>("id2");
                var a2 = Context.Find<IIdentity>(a1.Ref);
                var b2 = Context.Find<IIdentity>(b1.Ref);
                var c = Context.GetCompletion<Guid[]>();
                var tasks = new List<Task<Guid>> {
                    a1.Proxy.GetIdentity(), 
                    a2.Proxy.GetIdentity(), 
                    b1.Proxy.GetIdentity(), 
                    b2.Proxy.GetIdentity()
                };
                Task.Factory.ContinueWhenAll(tasks.Cast<Task>().ToArray(), _ => c.Complete(tasks.Select(x =>x.Result).ToArray()));
                return c;
            }

            public Task<Tuple<Guid, ActorRef>> Can_create_child_actor() {
                var a = Context.Create<IIdentity>("id");
                var c = Context.GetCompletion<Tuple<Guid, ActorRef>>();
                a.Proxy.GetIdentity().ContinueWith(t => {
                    c.Complete(new Tuple<Guid, ActorRef>(t.Result, a.Ref));
                });
                return c;
            }
        }


        [Test]
        public void Can_find_actor_by_ref() {
            var stage = new Stage();
            var tester = stage.Create<IContextTester>().Proxy;
            var r = tester.Can_find_actor_by_ref().WaitForResult();
            var a1Id = r[0];
            var a2Id = r[1];
            var b1Id = r[2];
            var b2Id = r[3];
            Assert.AreNotEqual(a1Id, b1Id, "actor ids were the same");
            Assert.AreEqual(a1Id, a2Id, "a ids did not match");
            Assert.AreEqual(b1Id, b2Id, "b ids did not match");

        }

        [Test]
        public void Can_create_child_actor() {
            var stage = new Stage();
            var tester = stage.Create<IContextTester>("tester").Proxy;
            var x = tester.Can_create_child_actor().WaitForResult();
            Debug.WriteLine("ref: {0}", x.Item2);
            var a = stage.Find<IIdentity>(x.Item2);
            var id = a.Proxy.GetIdentity().WaitForResult();
            Assert.AreEqual(id, x.Item1);
        }
    }
}