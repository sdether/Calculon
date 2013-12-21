using System;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Droog.Calculon.Backstage {
    public class ActorProxyInterceptor<TActor> : IInterceptor
        where TActor : class {
        private readonly IMailbox _sender;
        private readonly IMailbox<TActor> _receiver;

        public ActorProxyInterceptor(IMailbox sender, IMailbox<TActor> receiver) {
            _sender = sender;
            _receiver = receiver;
        }

        public void Intercept(IInvocation invocation) {
            var methodInfo = invocation.Method;
            var returnType = methodInfo.ReturnType;
            MessageType messageType;
            Type responseType = null;
            var id = Guid.Empty;
            if(returnType == typeof(void)) {
                messageType = MessageType.FireAndForget;
            } else if(returnType == typeof(Task)) {
                messageType = MessageType.Notification;
                var response = _sender.CreatePendingResponse(typeof(object));
                id = response.Id;
                invocation.ReturnValue = response.Task;
            } else {
                messageType = MessageType.Result;
                responseType = returnType.GetGenericArguments().First();
                 var response = _sender.CreatePendingResponse(responseType);
                 id = response.Id;
                 invocation.ReturnValue = response.Task;
            }
            var args = Enumerable.Range(0, methodInfo.GetParameters().Length).Select(invocation.GetArgumentValue).ToArray();
            _receiver.Enqueue(new Message(id, _sender.Ref, Message.GetContractFromMethodInfo(methodInfo), messageType, responseType, args));

        }
    }
}