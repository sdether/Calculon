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
using System.Threading;
using System.Threading.Tasks;
using Droog.Calculon.Tests.Actors;
using NUnit.Framework;

namespace Droog.Calculon.Tests {
    [TestFixture]
    public class ActorConcurrencyTests {

        [Test]
        public void Can_make_many_sequential_calls() {
            var stage = new Stage();
            var a1 = stage.Create<IAdder>().Proxy;

            var a1Tasks = Enumerable.Range(0, 10).Select(x => a1.Add(x, 10)).ToArray();
            Console.WriteLine("all tasks started");
            Task.WaitAll(a1Tasks);
            Console.WriteLine("all tasks completed");
            for(var i = 0; i < 10; i++) {
                Assert.AreEqual(10 + i, a1Tasks[i].Result);

            }
        }

        [Test]
        public void Can_make_many_concurrent_calls() {
            var stage = new Stage();
            var a1 = stage.Create<IAdder>().Proxy;

            var start = new ManualResetEvent(false);
            var ready = new List<ManualResetEvent>();
            var tasks = new List<Task<int>>();
            for(var i = 0; i < 10; i++) {
                var i2 = i;
                var mre = new ManualResetEvent(false);
                ready.Add(mre);
                var tcs = new TaskCompletionSource<int>();
                tasks.Add(tcs.Task);
                new Thread(() => {
                    mre.Set();
                    start.WaitOne();
                    a1.Add(i2, 10).ContinueWith(t => tcs.SetResult(t.Result));
                }).Start();
            }
            WaitHandle.WaitAll(ready.ToArray());
            start.Set();
            Task.WaitAll(tasks.ToArray());
            Debug.WriteLine("all tasks completed");
            for(var i = 0; i < 10; i++) {
                Assert.AreEqual(10 + i, tasks[i].Result);

            }
        }

        [Test]
        public void Only_one_message_can_enter_method() {
            var stage = new Stage();
            var a1 = stage.Create<IConcurrently>().Proxy;

            var start = new ManualResetEvent(false);
            var ready = new List<ManualResetEvent>();
            var tasks = new List<Task<ConcurrentResult>>();
            var n = 20;
            for(var i = 0; i < n; i++) {
                var i2 = i;
                var mre = new ManualResetEvent(false);
                ready.Add(mre);
                var tcs = new TaskCompletionSource<ConcurrentResult>();
                tasks.Add(tcs.Task);
                new Thread(() => {
                    mre.Set();
                    start.WaitOne();
                    a1.OnlyOneShallEnter(TimeSpan.FromMilliseconds(200)).ContinueWith(t => {
                        var r = t.Result;
                        Debug.WriteLine("{0}: {1}->{2}->{3}", i2, r.Start, r.During, r.End);
                        tcs.SetResult(t.Result);
                    });
                }).Start();
            }
            WaitHandle.WaitAll(ready.ToArray());
            start.Set();
            Task.WaitAll(tasks.ToArray());
            Debug.WriteLine("all tasks completed");
            for(var i = 0; i < n; i++) {
                var r = tasks[i].Result;
                Debug.WriteLine("{0}: {1}->{2}->{3}", i, r.Start, r.During, r.End);
                Assert.AreEqual(0, tasks[i].Result.Start);
                Assert.AreEqual(1, tasks[i].Result.During);
                Assert.AreEqual(0, tasks[i].Result.End);
            }
        }
        [Test]
        public void Only_one_message_can_enter_actor() {
            var stage = new Stage();
            var a1 = stage.Create<IConcurrently>().Proxy;

            var start = new ManualResetEvent(false);
            var ready = new List<ManualResetEvent>();
            var tasks = new List<Task<ConcurrentResult>>();
            var n = 20;
            for(var i = 0; i < n; i++) {
                var i2 = i;
                var mre = new ManualResetEvent(false);
                ready.Add(mre);
                var tcs = new TaskCompletionSource<ConcurrentResult>();
                tasks.Add(tcs.Task);
                new Thread(() => {
                    mre.Set();
                    start.WaitOne();
                    (i2 % 2 == 0 ? a1.OnlyOneShallEnter(TimeSpan.FromMilliseconds(200)) : a1.OnlyOneShallEnter2(TimeSpan.FromMilliseconds(50)))
                        .ContinueWith(t => {
                            var r = t.Result;
                            Debug.WriteLine("{0}: {1}->{2}->{3}", i2, r.Start, r.During, r.End);
                            tcs.SetResult(t.Result);
                        });
                }).Start();
            }
            WaitHandle.WaitAll(ready.ToArray());
            start.Set();
            Task.WaitAll(tasks.ToArray());
            Debug.WriteLine("all tasks completed");
            for(var i = 0; i < n; i++) {
                var r = tasks[i].Result;
                Debug.WriteLine("{0}: {1}->{2}->{3}", i, r.Start, r.During, r.End);
                Assert.AreEqual(0, tasks[i].Result.Start);
                Assert.AreEqual(1, tasks[i].Result.During);
                Assert.AreEqual(0, tasks[i].Result.End);
            }
        }
    }
}