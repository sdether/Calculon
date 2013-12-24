using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Droog.Calculon.Tests {
    [TestFixture, Ignore]
    public class EstimatingPi {
        public interface IWorker {
            Task<double> Range(int @from, int to);
        }

        public class Worker : AActor, IWorker {

            public Task<double> Range(int @from, int to) {
                var value = Enumerable.Range(from, to - from).Select(x => 4 * Math.Pow(-1, x) / (2 * x + 1)).Sum();
                return Return(value);
            }
        }

        public class LoadBalancer : AActor, IWorker {
            private readonly int _workerCount;
            private IWorker[] _workers;
            private int _currentWorker = 0;

            public LoadBalancer(int workerCount) {
                _workerCount = workerCount;
            }

            public Task<double> Range(int @from, int to) {
                VerifyInitialization();
                var completion = Context.GetCompletion<double>();
                _workers[_currentWorker].Range(from, to).ContinueWith(completion.Forward);
                _currentWorker++;
                if(_currentWorker == _workers.Length) {
                    _currentWorker = 0;
                }
                return completion;
            }

            private void VerifyInitialization() {
                if(_workers == null) {
                    _workers = Enumerable.Range(0, _workerCount)
                        .Select(
                            x => Context.Create<IWorker>(
                                name: "worker_" + x,
                                builder: () => new Worker()
                            ).Proxy
                        )
                        .ToArray();
                }
            }
        }

        public interface IAccumulator {
            Task<Tuple<double, TimeSpan>> Start();
            void Accumulate(double value);
        }

        public class Accumulator : AActor, IAccumulator {
            private readonly int _iterations;
            private Completion<Tuple<double, TimeSpan>> _completion;
            private Stopwatch _t;
            private int _count = 0;
            private double _pi;

            public Accumulator(int iterations) {
                _iterations = iterations;
            }

            public Task<Tuple<double, TimeSpan>> Start() {
                if(_completion != null) {
                    throw new Exception("accumulator already started");
                }
                _t = Stopwatch.StartNew();
                _completion = Context.GetCompletion<Tuple<double, TimeSpan>>();
                return _completion;
            }

            public void Accumulate(double value) {
                _count++;
                _pi += value;
                if(_iterations == _count) {
                    _t.Stop();
                    _completion.Complete(new Tuple<double, TimeSpan>(_pi, _t.Elapsed));
                }
            }
        }

        [Test]
        public void One_workers() {
            Run(1);
        }


        [Test]
        public void Two_workers() {
            Run(2);
        }


        [Test]
        public void Four_workers() {
            Run(4);
        }


        [Test]
        public void Eight_workers() {
            Run(8);
        }


        [Test]
        public void Sixteen_workers() {
            Run(16);
        }


        private static void Run(int workers) {
            var iterations = 100000;
            var length = 10000;
            var stage = new Stage();
            var worker = stage.Create<IWorker>(builder: () => new LoadBalancer(workers)).Proxy;
            var accumulator = stage.Create<IAccumulator>(builder: () => new Accumulator(iterations)).Proxy;
            var t = accumulator.Start();
            for(var i = 0; i < iterations; i++) {
                worker.Range(i * length, (i + 1) * length - 1).ContinueWith(t1 => accumulator.Accumulate(t1.Result));
            }
            var result = t.WaitForResult();
            Console.WriteLine("Workers:  {0}", workers);
            Console.WriteLine("Run time: {0}ms", result.Item2.TotalMilliseconds);
            Console.WriteLine("Pi:       {0}", result.Item1);
        }
    }
}