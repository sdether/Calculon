using System.Threading.Tasks;

namespace Droog.Calculon.Backstage {
    public static class TaskHelpers {
        public static readonly Task CompletedTask;

        static TaskHelpers() {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            CompletedTask = tcs.Task;
        }

        public static Task<TResult> GetCompletedTask<TResult>(TResult value) {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetResult(value);
            return tcs.Task;
        }

        //public static Task GetCompletedTask() {
        //    var tcs = new TaskCompletionSource<object>();
        //    tcs.SetResult(null);
        //    return tcs.Task;
        //}
    }
}