using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Droog.Calculon.Framework {
    public class StageTaskScheduler : TaskScheduler {
        private readonly ConcurrentQueue<Task> _queue = new ConcurrentQueue<Task>();
 
        protected override void QueueTask(Task task) {
            Console.WriteLine("queueing task");
            //_queue.Enqueue(task);
            TryExecuteTask(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks() {
            return _queue.ToArray();
        }
    }
}