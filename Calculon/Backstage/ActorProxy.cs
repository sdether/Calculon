using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Droog.Calculon.Backstage {
    public class ActorProxy<TActor> : IInterceptor
        where TActor : class {
        private readonly IMailbox _sender;
        private readonly IMailbox<TActor> _receiver;
        private readonly MethodInfo _interceptTaskInfo;

        public ActorProxy(IMailbox sender, IMailbox<TActor> receiver) {
            _sender = sender;
            _receiver = receiver;
            _interceptTaskInfo = GetType().GetMethods(BindingFlags.NonPublic|BindingFlags.Instance).First(x => x.Name == "InterceptTask" && x.IsGenericMethod);
        }

        public void Intercept(IInvocation invocation) {
            var methodInfo = invocation.Method;
            var returnType = methodInfo.ReturnType;
            if(returnType == typeof(void)) {
                InterceptVoid(invocation);
            } else if(returnType == typeof(Task)) {
                InterceptTask(invocation);
            } else {
                var taskType = returnType.GetGenericArguments().First();
                var generic = _interceptTaskInfo.MakeGenericMethod(taskType);
                generic.Invoke(this, new object[] {invocation});
            }
        }

        private void InterceptTask<TResult>(IInvocation invocation) {
            var methodInfo = invocation.Method;
            var args = Enumerable.Range(0, methodInfo.GetParameters().Length).Select(invocation.GetArgumentValue).ToArray();
            Func<TActor, Task<TResult>> func = actor => methodInfo.Invoke(actor, args) as Task<TResult>;
            var response = _sender.CreatePendingResponse<TResult>();
            _receiver.EnqueueExpression(response.Id, _sender.Ref, func);
            invocation.ReturnValue = response.Task;
        }

        private void InterceptTask(IInvocation invocation) {
            var methodInfo = invocation.Method;
            var args = Enumerable.Range(0, methodInfo.GetParameters().Length).Select(invocation.GetArgumentValue).ToArray();
            Func<TActor, Task> func = actor => methodInfo.Invoke(actor, args) as Task;
            var response = _sender.CreatePendingResponse<int>();
            _receiver.EnqueueExpression(response.Id, _sender.Ref, func);
            invocation.ReturnValue = response.Task;
        }

        private void InterceptVoid(IInvocation invocation) {
            var methodInfo = invocation.Method;
            var args = Enumerable.Range(0, methodInfo.GetParameters().Length).Select(invocation.GetArgumentValue).ToArray();
            Action<TActor> action = actor => methodInfo.Invoke(actor, args);
            _receiver.EnqueueExpression(Guid.NewGuid(), _sender.Ref, action);
        }
    }
}