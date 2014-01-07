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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Droog.Calculon.Backstage.Messages;

namespace Droog.Calculon.Backstage {
    public class Mailbox<TActor> : IMailbox where TActor : class {

        private enum ProcessingState {
            Idle,
            Processing,
            Suspended,
            Dead
        }

        private delegate bool Handler(Message message, TActor instance, Action<Message> response);

        private readonly ActorRef _parent;
        private readonly List<ActorRef> _children = new List<ActorRef>();
        private readonly IBackstage _backstage;
        private readonly Func<TActor> _builder;
        private readonly ActorRef _actorRef;
        private readonly MessageQueue _queue = new MessageQueue();
        private readonly Dictionary<Guid, MessageResponse> _pendingResponses = new Dictionary<Guid, MessageResponse>();
        private readonly Dictionary<string, Handler> _handlers = new Dictionary<string, Handler>();
        private readonly MethodInfo _buildTaskofTHandler;
        private readonly TActor _instance;
        private ProcessingState _processing = ProcessingState.Idle;

        public Mailbox(ActorRef parent, string name, IBackstage backstage, Func<TActor> builder) {
            _parent = parent;
            _backstage = backstage;
            _builder = builder;
            _actorRef = (parent ?? ActorRef.Parse("/")).At(name);
            _instance = _builder();
            _buildTaskofTHandler = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == "BuildAskHandler" && x.IsGenericMethod);
            var actor = _instance as IActor;
            actor.Context = new ActorContext(_backstage, Ref, _parent);
            PopulatedMessageHandlers();
            _backstage.Enqueue(new CreatedMessage(Ref, _parent));
        }

        private void PopulatedMessageHandlers() {
            foreach(var methodInfo in typeof(TActor).GetMethods()) {
                var returnType = methodInfo.ReturnType;
                Handler handler;
                if(returnType == typeof(void)) {
                    handler = BuildTellHandler(methodInfo);
                } else if(returnType == typeof(Task)) {
                    handler = BuildNotificationHandler(methodInfo);
                } else if(returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)) {
                    var taskType = returnType.GetGenericArguments().First();
                    var generic = _buildTaskofTHandler.MakeGenericMethod(taskType);
                    handler = generic.Invoke(this, new object[] { methodInfo }) as Handler;
                } else {
                    continue;
                }
                var signature = Message.GetMessageNameFromMethodInfo(methodInfo);
                _handlers[signature] = handler;
            }
            _handlers[SystemMessageNames.Response.ToString()] = ResponseHandler;
            _handlers[SystemMessageNames.Fault.ToString()] = FaultHandler;
            _handlers[SystemMessageNames.Created.ToString()] = ChildCreationHandler;
            _handlers[SystemMessageNames.Suspend.ToString()] = SuspendHandler;
            _handlers[SystemMessageNames.Resume.ToString()] = ResumeHandler;
        }

        private bool FaultHandler(Message msg, TActor actor, Action<Message> callback) {

            // TODO: deal with this coming up null
            var failureMessage = msg as FailureMessage;
            var response = GetPendingResponse(failureMessage.Id);
            if(response == null) {
                return false;
            }
            response.Fault(failureMessage.Exception);
            return true;
        }

        private bool ResponseHandler(Message msg, TActor actor, Action<Message> callback) {

            // TODO: deal with this coming up null
            var responseMessage = msg as ResponseMessage;
            var response = GetPendingResponse(responseMessage.Id);
            if(response == null) {
                return false;
            }
            response.Complete(responseMessage.Response);
            return true;
        }

        private bool ChildCreationHandler(Message msg, TActor actor, Action<Message> callback) {
            return true;
        }


        private bool SuspendHandler(Message msg, TActor actor, Action<Message> callback) {
            return true;
        }

        private bool ResumeHandler(Message msg, TActor actor, Action<Message> callback) {
            return true;
        }

        private Handler BuildNotificationHandler(MethodInfo methodInfo) {
            return (m, actor, callback) => {

                // TODO: deal with this coming up null
                var msg = m as NotificationMessage;
                try {
                    (methodInfo.Invoke(actor, msg.Args) as Task).ContinueWith(t => {
                        if(!t.IsFaulted) {
                            callback(new ResponseMessage(null, m));
                            return;
                        }
                        var e = t.Exception.Flatten() as Exception;
                        var fatal = false;
                        if(e.GetType() == typeof(FatalActorException)) {
                            e = e.InnerException;
                            fatal = true;
                        }
                        callback(new FailureMessage(e, m, fatal));
                    });
                } catch(TargetInvocationException e) {
                    callback(new FailureMessage(e.InnerException, m, fatal: true));
                } catch(Exception e) {
                    callback(new FailureMessage(e, m));
                }

                return true;
            };
        }

        private Handler BuildAskHandler<TResult>(MethodInfo methodInfo) {
            return (m, actor, callback) => {

                // TODO: deal with this coming up null
                var msg = m as AskMessage;
                try {
                    (methodInfo.Invoke(actor, msg.Args) as Task<TResult>).ContinueWith(t => {
                        if(!t.IsFaulted) {
                            callback(new ResponseMessage(t.Result, m));
                            return;
                        }
                        var e = t.Exception.Flatten() as Exception;
                        var fatal = false;
                        if(e.GetType() == typeof(FatalActorException)) {
                            e = e.InnerException;
                            fatal = true;
                        }
                        callback(new FailureMessage(e, m, fatal));
                    });
                } catch(TargetInvocationException e) {
                    callback(new FailureMessage(e.InnerException, m, fatal: true));
                } catch(Exception e) {
                    callback(new FailureMessage(e, m));
                }
                return true;
            };
        }

        private Handler BuildTellHandler(MethodInfo methodInfo) {
            return (m, actor, callback) => {

                // TODO: deal with this coming up null
                var msg = m as TellMessage;
                try {
                    methodInfo.Invoke(actor, msg.Args);
                } catch(TargetInvocationException e) {
                    callback(new FailureMessage(e.InnerException, m, fatal: true));
                } catch(Exception e) {
                    callback(new FailureMessage(e, m));
                }
                return true;
            };
        }

        private MessageResponse GetPendingResponse(Guid id) {
            lock(_pendingResponses) {
                MessageResponse response;
                if(_pendingResponses.TryGetValue(id, out response)) {
                    _pendingResponses.Remove(id);
                }
                return response;
            }
        }

        public ActorRef Ref { get { return _actorRef; } }

        public bool IsMailboxFor<TActor1>() {
            return typeof(TActor).IsAssignableFrom(typeof(TActor1));
        }

        public MessageResponse CreatePendingResponse(Type type) {
            var response = new MessageResponse(type);
            lock(_pendingResponses) {
                _pendingResponses.Add(response.Id, response);
            }
            return response;
        }

        public void Enqueue(Message msg) {
            lock(_queue) {
                _queue.Enqueue(msg);
                if(_processing != ProcessingState.Idle) {
                    return;
                }
                _processing = ProcessingState.Processing;
                ThreadPool.QueueUserWorkItem(Dequeue);
            }
        }

        private void Dequeue(object state) {

            // TODO: should be able peek ahead in the queue and reduce the need for two locks per message
            // (just gets tricky when actor is suspended)
            while(_processing == ProcessingState.Processing) {
                MessageQueue.QueueItem msg = null;
                msg = _queue.Dequeue();
                if(msg == null) {
                    _processing = ProcessingState.Idle;
                    return;
                }
                ExecuteMessage(msg);
                if(_processing == ProcessingState.Suspended) {
                    return;
                }
            }
        }

        private void ExecuteMessage(MessageQueue.QueueItem item) {
            var msg = item.Message;
            var actor = _instance as IActor;
            actor.Sender = msg.Sender;
            Handler handler;
            if(!_handlers.TryGetValue(msg.Name, out handler)) {

                // TODO: deliver via generic message handler
                return;
            }
            if(!handler(msg, _instance, responseMsg => {

                // TODO: propagate failures to parent and defer sending them on to the original sender
                if(responseMsg.IsFatalFault) {
                    Suspend();
                    _backstage.Enqueue(responseMsg.Wrap(Ref, _parent));
                }
                _backstage.Enqueue(responseMsg);
                item.Complete();
            })) {

                // TODO: deliver via generic message handler
            };
        }

        private void Suspend() {
            _processing = ProcessingState.Suspended;
            foreach(var child in _children) {
                _backstage.Enqueue(new SuspendMessage(Ref, child));
            }
        }

        private void Resume() {
            _processing = ProcessingState.Processing;
            foreach(var child in _children) {
                _backstage.Enqueue(new ResumeMessage(Ref, child));
            }
        }

        public void Terminate() {
            throw new NotImplementedException();
        }

        public void Restart() {
            throw new NotImplementedException();
        }
    }
}