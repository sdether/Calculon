using System.Threading.Tasks;

namespace Droog.Calculon.Tests {
    public static class TaskHelpers {
        
        public static Task<TResult> Block<TResult>(this Task<TResult> task) {
            task.Wait();
            return task;
        }

        public static TResult WaitForResult<TResult>(this Task<TResult> task) {
            return task.Block().Result;
        }
    }
}