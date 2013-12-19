using System;
using System.Threading.Tasks;

namespace Droog.Calculon {
    public class Completion<TResult> {
        public static implicit operator Task<TResult>(Completion<TResult> completion) {
            return completion._completionSource.Task;
        }

        private readonly TaskCompletionSource<TResult> _completionSource = new TaskCompletionSource<TResult>();

        public void SetResult(TResult result) {
            _completionSource.SetResult(result);
        }

        public void SetException(Exception exception) {
            _completionSource.SetException(exception);
        }
    }
}