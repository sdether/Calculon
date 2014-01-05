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
        private readonly ActorRef _receiver;
        private readonly IBackstage _backstage;

        public ActorProxyInterceptor(IMailbox sender, ActorRef receiver, IBackstage backstage) {
            _sender = sender;
            _receiver = receiver;
            _backstage = backstage;
        }

        public void Intercept(IInvocation invocation) {
            var methodInfo = invocation.Method;
            var returnType = methodInfo.ReturnType;
            Message message;
            var args = Enumerable.Range(0, methodInfo.GetParameters().Length).Select(invocation.GetArgumentValue).ToArray();
            var name = Message.GetMessageNameFromMethodInfo(methodInfo);
            if(returnType == typeof(void)) {
                message = new TellMessage(name,_sender.Ref, _receiver, args);
            } else if(returnType == typeof(Task)) {
                var response = _sender.CreatePendingResponse(typeof(object));
                message = new NotificationMessage(response.Id, name, _sender.Ref, _receiver, args);
                invocation.ReturnValue = response.Task;
            } else if(returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)) {
                var responseType = returnType.GetGenericArguments().First();
                var response = _sender.CreatePendingResponse(responseType);
                message = new AskMessage(response.Id, name, _sender.Ref, _receiver, args);
                invocation.ReturnValue = response.Task;
            } else {
                throw new InvalidOperationException(string.Format("Method '{0}' is not a valid message signature", methodInfo.Name));
            }
            _backstage.Enqueue(message);
        }
    }
}