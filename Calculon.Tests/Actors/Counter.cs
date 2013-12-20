using System.Threading.Tasks;

namespace Droog.Calculon.Tests.Actors {
    public interface ICounter {
        Task Increment();
        Task<int> Current();
    }
    
    public class Counter : AActor, ICounter {

        private int _counter;
        public Task Increment() {
            ++_counter;
            return Context.Return();
        }

        public Task<int> Current() {
            return Context.Return(_counter);
        }
    }
}