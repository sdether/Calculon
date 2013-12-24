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
    [TestFixture, Ignore]
    public class MessagingPerfTests {

        [Test]
        public void SequentialThroughput() {
            var stage = new Stage();
            var a = stage.Create<IPerf>().Proxy;
            var n = 50000;
            var t = Stopwatch.StartNew();
            for(var i = 0; i < n; i++) {
                a.Respond(i, "foo").Wait();
            }
            t.Stop();
            Console.WriteLine("rate: {0:0.00}calls/s", n / t.Elapsed.TotalSeconds);
        }

        [Test]
        public void SemiSequentialThroughput() {
            var stage = new Stage();
            var a = stage.Create<IPerf>().Proxy;
            var n = 50000;
            var t = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for(var i = 0; i < n; i++) {
                tasks.Add(a.Respond(i, "foo"));
            }
            Task.WaitAll(tasks.ToArray());
            t.Stop();
            Console.WriteLine("rate: {0:0.00}calls/s", n / t.Elapsed.TotalSeconds);
        }

        [Test]
        public void SemiSequentialViaManyTargets() {
            var stage = new Stage();
            var m = 10;
            var a = Enumerable.Range(0, m).Select(x => stage.Create<IPerf>(name: x.ToString()).Proxy).ToArray();
            var n = 50000;
            var t = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for(var i = 0; i < n / m; i++) {
                for(var j = 0; j < m; j++) {
                    tasks.Add(a[j].Respond(i, "foo"));
                }
            }
            Task.WaitAll(tasks.ToArray());
            t.Stop();
            Console.WriteLine("rate: {0:0.00}calls/s", n * m / t.Elapsed.TotalSeconds);
        }

        [Test]
        public void SequentialViaManyTargets() {
            var stage = new Stage();
            var m = 10;
            var a = Enumerable.Range(0, m).Select(x => stage.Create<IPerf>(name: x.ToString()).Proxy).ToArray();
            var n = 10000;
            var t = Stopwatch.StartNew();
            for(var i = 0; i < n / m; i++) {
                for(var j = 0; j < m; j++) {
                    a[j].Respond(i, "foo").Wait();
                }
            }
            t.Stop();
            Console.WriteLine("rate: {0:0.00}calls/s", n * m / t.Elapsed.TotalSeconds);
        }

        [Test]
        public void FireAndForgetThroughput() {
            var stage = new Stage();
            var a = stage.Create<IPerf>().Proxy;
            var n = 50000;
            var t = Stopwatch.StartNew();
            var tx = a.SignalCounter(n).ContinueWith(_ => t.Stop());
            for(var i = 0; i < n; i++) {
                a.Increment();
            }
            tx.Wait();
            t.Stop();
            Console.WriteLine("rate: {0:0.00}calls/s", n / t.Elapsed.TotalSeconds);
        }

        [Test]
        public void Sequential_from_many_callers() {
            var stage = new Stage();
            var a = stage.Create<IPerf>().Proxy;
            var start = new ManualResetEvent(false);
            var ready = new List<ManualResetEvent>();
            var tasks = new List<Task<int>>();
            var n = 10000;
            var m = 10;
            for(var i = 0; i < m; i++) {
                var mre = new ManualResetEvent(false);
                ready.Add(mre);
                var tcs = new TaskCompletionSource<int>();
                tasks.Add(tcs.Task);
                new Thread(() => {
                    mre.Set();
                    start.WaitOne();
                    for(var j = 0; j < n; j++) {
                        a.Respond(j, "foo").Wait();
                    }
                    mre.Set();
                }).Start();
            }
            WaitHandle.WaitAll(ready.ToArray());
            foreach(var mre in ready) {
                mre.Reset();
            }
            var t = Stopwatch.StartNew();
            start.Set();
            WaitHandle.WaitAll(ready.ToArray());
            t.Stop();
            Console.WriteLine("rate: {0:0.00}calls/s", n * m / t.Elapsed.TotalSeconds);
        }

        [Test]
        public void Ask_vs_tell_sequential() {
            var stage = new Stage();
            var perf = stage.Create<IPerfSender>().Proxy;
            var n = 100000;
            var ask = perf.AskSequential(n).WaitForResult();
            Console.WriteLine("{0:0.00} asks/s", n / ask.TotalSeconds);
            var tell = perf.TellSequential(n).WaitForResult();
            Console.WriteLine("{0:0.00} tells/s", n / tell.TotalSeconds);
        }

        [Test]
        public void Ask_vs_tell_parallel() {
            var stage = new Stage();
            var perf = stage.Create<IPerfSender>().Proxy;
            var n = 100000;
            var ask = perf.AskParallel(n).WaitForResult();
            Console.WriteLine("{0:0.00} asks/s", n / ask.TotalSeconds);
            var tell = perf.TellParallel(n).WaitForResult();
            Console.WriteLine("{0:0.00} tells/s", n / tell.TotalSeconds);
        }
    }
}