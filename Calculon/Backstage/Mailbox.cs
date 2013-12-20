using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Droog.Calculon.Backstage {
    public class Mailbox<TActor> : IMailbox<TActor> where TActor : class {

        private static readonly ProxyGenerator PROXY_GENERATOR = new ProxyGenerator();
        private static readonly int MAX_PROCESSNG_SIZE = 100;
        private readonly ActorRef _parent;
        private readonly IBackstage _backstage;
        private readonly Func<TActor> _builder;
        private readonly ActorRef _actorRef;
        private readonly Queue<Message<TActor>> _queue = new Queue<Message<TActor>>();
        private readonly Dictionary<Guid, object> _pendingResponses = new Dictionary<Guid, object>();
        private TActor _instance;
        private bool _processing = false;

        public Mailbox(ActorRef parent, string name, IBackstage backstage, Func<TActor> builder) {
            _parent = parent;
            _backstage = backstage;
            _builder = builder;
            _actorRef = (parent ?? ActorRef.Parse("/")).At(name);
            _instance = _builder();
            var actor = _instance as IActor;
            actor.Context = new ActorContext(_backstage, Ref, _parent);

        }

        public ActorRef Ref { get { return _actorRef; } }

        public bool IsMailboxFor<TActor1>() {
            return typeof(TActor).IsAssignableFrom(typeof(TActor1));
        }

        public IMailbox<TActor1> As<TActor1>() where TActor1 : class {
            return this as IMailbox<TActor1>;
        }

        public MessageResponse<TResult> CreatePendingResponse<TResult>() {
            var tcs = new TaskCompletionSource<TResult>();
            var response = new MessageResponse<TResult>(tcs);
            _pendingResponses.Add(response.Id, tcs);
            return response;
        }

        public void EnqueueExpression<TResult>(Guid id, ActorRef sender, Func<TActor, Task<TResult>> expr) {
            var msg = new Message<TActor>(sender, (backstage, actor) => {
                var tcs = new TaskCompletionSource<object>();
                try {
                    expr(actor).ContinueWith(t => {
                        var mailbox = backstage.GetMailbox(sender);
                        mailbox.EnqueueResponseMessage(id, Ref, t);
                        tcs.SetResult(null);
                    });
                } catch(TargetInvocationException e) {
                    var mailbox = backstage.GetMailbox(sender);
                    mailbox.EnqueueResponseMessage(id, Ref, TaskHelpers.GetFaultedTask<TResult>(e.InnerException));
                }
                return tcs.Task;
            });

            Enqueue(msg);
        }

        public void EnqueueExpression(Guid id, ActorRef sender, Func<TActor, Task> expr) {
            var msg = new Message<TActor>(sender, (backstage, actor) => {
                var tcs = new TaskCompletionSource<object>();
                try {
                    expr(actor).ContinueWith(t => {
                        var mailbox = backstage.GetMailbox(sender);
                        mailbox.EnqueueResponseMessage(id, Ref, t);
                        tcs.SetResult(null);
                    });
                } catch(TargetInvocationException e) {
                    var mailbox = backstage.GetMailbox(sender);
                    mailbox.EnqueueResponseMessage(id, Ref, TaskHelpers.GetFaultedTask(e.InnerException));
                }
                return tcs.Task;
            });
            Enqueue(msg);
        }

        public void EnqueueExpression(Guid id, ActorRef sender, Action<TActor> expr) {
            Enqueue(new Message<TActor>(sender, (backstage, actor) => {
                expr(actor);
                return TaskHelpers.CompletedTask;
            }));
        }

        public void EnqueueResponseMessage<TResult>(Guid id, ActorRef sender, Task<TResult> result) {
            object completionObject;
            if(!_pendingResponses.TryGetValue(id, out completionObject)) {
                return;
            }
            var completion = completionObject as TaskCompletionSource<TResult>;
            if(completion == null) {
                return;
            }
            Enqueue(new Message<TActor>(sender, (backstage, actor) => {
                if(result.IsFaulted) {
                    completion.SetException(result.Exception);
                } else {
                    completion.SetResult(result.Result);
                }
                return TaskHelpers.CompletedTask;
            }));
        }

        public void EnqueueResponseMessage(Guid id, ActorRef sender, Task result) {
            object completionObject;
            if(!_pendingResponses.TryGetValue(id, out completionObject)) {
                return;
            }
            var completion = completionObject as TaskCompletionSource<object>;
            if(completion == null) {
                return;
            }
            Enqueue(new Message<TActor>(sender, (backstage, actor) => {
                if(result.IsFaulted) {
                    completion.SetException(result.Exception);
                } else {
                    completion.SetResult(null);
                }
                return TaskHelpers.CompletedTask;
            }));
        }

        public void SetInstance(TActor instance) {
            _instance = instance;
        }

        public TActor BuildProxy(IMailbox sender) {
            return PROXY_GENERATOR.CreateInterfaceProxyWithoutTarget<TActor>(new ActorProxyInterceptor<TActor>(sender, this));
        }

        private void Enqueue(Message<TActor> message) {
            lock(_queue) {
                _queue.Enqueue(message);
                if(_processing) {
                    return;
                }
                _processing = true;
                ThreadPool.QueueUserWorkItem(Dequeue);
            }
        }

        private void Dequeue(object state) {
            List<Message<TActor>> manyMsg = null;
            Message<TActor> singleMsg = null;
            var size = 0;
            lock(_queue) {
                size = _queue.Count;
                switch(size) {
                case 0:
                    _processing = false;
                    return;
                case 1:
                    singleMsg = _queue.Dequeue();
                    break;
                default:
                    manyMsg = new List<Message<TActor>>();
                    for(var i = 0; i < Math.Min(size, MAX_PROCESSNG_SIZE); i++) {
                        manyMsg.Add(_queue.Dequeue());
                    }
                    break;
                }
            }
            if(singleMsg != null) {
                ExecuteMessage(singleMsg);
            } else {
                foreach(var msg in manyMsg) {
                    ExecuteMessage(msg);
                }
                if(size > 10) {
                    ThreadPool.QueueUserWorkItem(Dequeue);
                    return;
                }
            }
            lock(_queue) {
                if(!_queue.Any()) {
                    _processing = false;
                    return;
                }
                ThreadPool.QueueUserWorkItem(Dequeue);
            }
        }

        private void ExecuteMessage(Message<TActor> msg) {
            var actor = _instance as IActor;
            actor.Sender = msg.Sender;
            msg.Execute(_backstage, _instance);
        }
    }
}