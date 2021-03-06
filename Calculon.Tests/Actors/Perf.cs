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
using System.Threading.Tasks;

namespace Droog.Calculon.Tests.Actors {
    public interface IPerf {
        Task<int> Respond(int a, string b);
        Task SignalCounter(int count);
        void Increment();
    }

    public class Perf : AActor, IPerf {
        private Completion _completion;
        private int _counter;

        public Task<int> Respond(int a, string b) {
            return Return(0);
        }

        public Task SignalCounter(int count) {
            _counter = count;
            _completion = Context.GetCompletion();
            return _completion;
        }

        public void Increment() {
            _counter--;
            if(_counter == 0) {
                _completion.Complete();
            }
        }
    }

    public interface IPerfSender {
        void TellResponse(Guid id, string data);
        Task<TimeSpan> AskParallel(int messageCount);
        Task<TimeSpan> AskSequential(int messageCount);
        Task<TimeSpan> TellParallel(int messageCount);
        Task<TimeSpan> TellSequential(int messageCount);
    }

    public class PerfSender : AActor, IPerfSender {
        private Completion<TimeSpan> _tellCompletion;
        private Stopwatch _tellTimer;
        private int _tellCount;
        private int _tellExpected;
        private bool _tellParallel;
        private IPerfReceiver _tellReceiver;

        public Task<TimeSpan> AskParallel(int messageCount) {
            var completion = Context.GetCompletion<TimeSpan>();
            var asks = new List<Task<string>>();
            var receiver = Context.Create<IPerfReceiver>().Proxy;
            var sw = Stopwatch.StartNew();
            for(var i = 0; i < messageCount; i++) {
                asks.Add(receiver.Ask("foo"));
            }
            Task.Factory.ContinueWhenAll(asks.ToArray(), _ => {
                sw.Stop();
                completion.Complete(sw.Elapsed);
            });
            return completion;
        }

        public Task<TimeSpan> AskSequential(int messageCount) {
            var completion = Context.GetCompletion<TimeSpan>();
            var receiver = Context.Create<IPerfReceiver>().Proxy;
            var sw = Stopwatch.StartNew();
            AskSequential(receiver, completion, sw, messageCount);
            return completion;
        }

        private void AskSequential(IPerfReceiver receiver, Completion<TimeSpan> completion, Stopwatch sw, int messageCount) {
            if(messageCount == 0) {
                sw.Stop();
                completion.Complete(sw.Elapsed);
            }
            receiver.Ask("foo").ContinueWith(t => AskSequential(receiver, completion, sw, messageCount - 1));
        }

        public Task<TimeSpan> TellParallel(int messageCount) {
            _tellParallel = true;
            _tellCount = 0;
            _tellExpected = messageCount;
            _tellTimer = Stopwatch.StartNew();
            _tellCompletion = Context.GetCompletion<TimeSpan>();
            var receiver = Context.Create<IPerfReceiver>().Proxy;
            for(var i = 0; i < messageCount; i++) {
                receiver.Tell(Guid.NewGuid(), "foo");
            }
            return _tellCompletion;
        }
        public Task<TimeSpan> TellSequential(int messageCount) {
            _tellParallel = false;
            _tellCount = 0;
            _tellExpected = messageCount;
            _tellTimer = Stopwatch.StartNew();
            _tellCompletion = Context.GetCompletion<TimeSpan>();
            _tellReceiver = Context.Create<IPerfReceiver>().Proxy;
            _tellReceiver.Tell(Guid.NewGuid(), "foo");
            return _tellCompletion;
        }

        public void TellResponse(Guid id, string data) {
            if(_tellParallel) {
                _tellCount++;
                if(_tellExpected == _tellCount) {
                    _tellTimer.Stop();
                    _tellCompletion.Complete(_tellTimer.Elapsed);
                    return;
                }
            } else {
                _tellCount++;
                if(_tellExpected == _tellCount) {
                    _tellTimer.Stop();
                    _tellCompletion.Complete(_tellTimer.Elapsed);
                    return;
                }
                _tellReceiver.Tell(Guid.NewGuid(), "foo");
            }
        }

    }

    public interface IPerfReceiver {
        Task<string> Ask(string data);
        void Tell(Guid id, string data);
    }

    public class PerfReceiver : AActor, IPerfReceiver {
        readonly Dictionary<string,IPerfSender> _tellTargetCache = new Dictionary<string, IPerfSender>();

        public Task<string> Ask(string data) {
            return Return(data);
        }

        public void Tell(Guid id, string data) {
            IPerfSender target;
            if(!_tellTargetCache.TryGetValue(Sender.ToString(), out target)) {
                _tellTargetCache[Sender.ToString()] = target = Context.Find<IPerfSender>(Sender).Proxy;
            }
            target.TellResponse(id, data);
        }
    }
}