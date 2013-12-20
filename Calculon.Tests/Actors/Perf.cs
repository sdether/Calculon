using System.Threading.Tasks;

namespace Droog.Calculon.Tests.Actors {
    public interface IPerf {
        Task<int> Respond(int a, string b);
        Task SignalCounter(int count);
        void Increment();
    }

    public class Perf :AActor, IPerf {
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
}