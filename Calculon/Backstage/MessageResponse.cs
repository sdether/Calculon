using System;
using System.Threading.Tasks;

namespace Droog.Calculon.Backstage {
    public class MessageResponse<TResult> {
        public readonly Task<TResult> Task;
        public readonly Guid Id;

        public MessageResponse(TaskCompletionSource<TResult> tcs) {
            Task = tcs.Task;
            Id = Guid.NewGuid();
        } 
    }
}