using System;
using System.Threading.Tasks;

namespace Droog.Calculon {

    public class Completion {
        public static implicit operator Task(Completion completion) {
            return completion._completionSource.Task;
        }

        private readonly TaskCompletionSource<object> _completionSource = new TaskCompletionSource<object>();

        public void Complete() {
            _completionSource.SetResult(null);
        }

        public void Fail(Exception exception) {
            _completionSource.SetException(exception);
        }
    }

    public class Completion<TResult> {
        public static implicit operator Task<TResult>(Completion<TResult> completion) {
            return completion._completionSource.Task;
        }

        private readonly TaskCompletionSource<TResult> _completionSource = new TaskCompletionSource<TResult>();

        public void Complete(TResult result) {
            _completionSource.SetResult(result);
        }

        public void Fail(Exception exception) {
            _completionSource.SetException(exception);
        }
    }
}