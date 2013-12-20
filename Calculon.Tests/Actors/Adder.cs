using System;
using System.Threading;
using System.Threading.Tasks;

namespace Droog.Calculon.Tests.Actors {
    public interface IAdder {
        Task<int> Add(int a, int b);
        Task<int> AddDelayed(int a, int b, TimeSpan delay);
    }
    public class Adder : AActor, IAdder {
        public Task<int> Add(int a, int b) {
            return Context.Return(a + b);
        }

        public Task<int> AddDelayed(int a, int b, TimeSpan delay) {
            var completion = Context.GetCompletion<int>();
            new Timer(_ => completion.Complete(a + b)).Change(delay, TimeSpan.Zero);
            return completion;
        }
    }
}