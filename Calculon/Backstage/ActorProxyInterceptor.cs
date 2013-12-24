/* ----------------------------------------------------------------------------
 * Copyright (C) 2013 Arne F. Claassen
 * geekblog [at] claassen [dot] net
 * http://github.com/sdether/Calculon
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 * ----------------------------------------------------------------------------
 */

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
            _receiver.Enqueue(new Message(id, _sender.Ref, _receiver.Ref, Message.GetContractFromMethodInfo(methodInfo), messageType, responseType, args));

        }
    }
}