using System;
using System.Threading;
using System.Threading.Tasks;

namespace Droog.Calculon.Tests.Actors {

    public class Failure : Exception {}

    public interface IFailing {
        Task FailImmediate();
        Task FailLater();
        Task FailByThrowing();
        Task<int> FailImmediateOfT();
        Task<int> FailLaterOfT();
        Task<int> FailByThrowingOfT();
        void VoidFailure();
        Task<int> VoidFailureCalls();
    }

    public class Failing : AActor, IFailing {
        private int _voidFailureCalled;

        public Task FailImmediate() {
            var completion = Context.GetCompletion();
            completion.Fail(new Failure());
            return completion;
        }

        public Task FailLater() {
            var completion = Context.GetCompletion();
            new Timer(_ => completion.Fail(new Failure()), null, 100, Timeout.Infinite);
            return completion;
        }

        public Task FailByThrowing() {
            throw new Failure();
        }

        public Task<int> FailImmediateOfT() {
            var completion = Context.GetCompletion<int>();
            completion.Fail(new Failure());
            return completion;
        }

        public Task<int> FailLaterOfT() {
            var completion = Context.GetCompletion<int>();
            new Timer(_ => completion.Fail(new Failure()), null, 100, Timeout.Infinite);
            return completion;
        }

        public Task<int> FailByThrowingOfT() {
            throw new Failure();
        }

        public void VoidFailure() {
            _voidFailureCalled++;
            throw new Failure();
        }

        public Task<int> VoidFailureCalls() {
            return Return(_voidFailureCalled);
        }
    }
}