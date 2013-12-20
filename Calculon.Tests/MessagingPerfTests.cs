using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Console.WriteLine("rate: {0:0.00}calls/s", n / t.Elapsed.TotalSeconds);
        }
    }
}