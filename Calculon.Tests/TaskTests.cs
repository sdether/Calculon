using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Droog.Calculon.Framework;
using NUnit.Framework;

namespace Droog.Calculon.Tests {
    
    [TestFixture]
    public class TaskTests {

        [Test]
        public void Can_intercept_continue() {
            var scheduler = new StageTaskScheduler();
            var factory = new TaskFactory(scheduler);
            var t = factory.StartNew(() => {
                    Console.WriteLine("start");
                    return 42;
                })
                .ContinueWith(t1 => {
                    Console.WriteLine("first continuation");
                    return t1.Result + 10;
                })
                .ContinueWith(t2 => {
                    Console.WriteLine("second continuation");
                    return t2.Result + 10;
                });
            t.Wait();
            Console.WriteLine("result: {0}",t.Result);
                
        }

        [Test]
        public void CompletionSource_schedule_continuation() {
            var tcs = new TaskCompletionSource<int>();
            var t = tcs.Task
                .ContinueWith(t1 => {
                    Console.WriteLine("first continuation");
                    return t1.Result + 10;
                })
                .ContinueWith(t2 => {
                    Console.WriteLine("second continuation");
                    return t2.Result + 10;
                });
            tcs.
            t.Wait();
            Console.WriteLine("result: {0}", t.Result);
        }
    }
}
