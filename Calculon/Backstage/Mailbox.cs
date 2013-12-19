using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Droog.Calculon.Backstage {
    public class Mailbox<TActor> : IMailbox<TActor> where TActor : class {

        private static ProxyGenerator _generator = new ProxyGenerator();

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
            _actorRef = new ActorRef(name, typeof(TActor));
            _instance = _builder();
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

        public void EnqueueExpression<TResult>(Guid id, Calculon.ActorRef sender, Func<TActor, Task<TResult>> expr) {
            var msg = new Message<TActor>(sender, (backstage, actor) => {
                var tcs = new TaskCompletionSource<object>();
                expr(actor).ContinueWith(t => {
                    var mailbox = backstage.GetMailbox(sender);
                    mailbox.EnqueueResponseMessage(id, Ref, t.Result);
                    tcs.SetResult(null);
                });
                return tcs.Task;
            });

            Enqueue(msg);
        }

        public void EnqueueExpression(Guid id, ActorRef sender, Func<TActor, Task> expr) {
            var msg = new Message<TActor>(sender, (backstage, actor) => {
                var tcs = new TaskCompletionSource<object>();
                expr(actor).ContinueWith(t => {
                    var mailbox = backstage.GetMailbox(sender);
                    mailbox.EnqueueResponseMessage<object>(id, Ref, null);
                    tcs.SetResult(null);
                });
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

        public void EnqueueResponseMessage<TResult>(Guid id, ActorRef sender, TResult result) {
            object completionObject;
            if(!_pendingResponses.TryGetValue(id, out completionObject)) {
                return;
            }
            var completion = completionObject as TaskCompletionSource<TResult>;
            if(completion == null) {
                return;
            }
            Enqueue(new Message<TActor>(sender, (backstage, actor) => {
                completion.SetResult(result);
                return TaskHelpers.CompletedTask;
            }));
        }

        public void SetInstance(TActor instance) {
            _instance = instance;
        }

        public TActor BuildProxy(IMailbox sender) {
            return _generator.CreateInterfaceProxyWithoutTarget<TActor>(new ActorProxy<TActor>(sender, this));
        }

        private void Enqueue(Message<TActor> message) {
            lock(_queue) {
                _queue.Enqueue(message);
                if(_processing) {
                    return;
                }
                ThreadPool.QueueUserWorkItem(Dequeue);
            }
        }

        private void Dequeue(object state) {
            List<Message<TActor>> manyMsg = null;
            Message<TActor> singleMsg = null;
            lock(_queue) {
                var size = _queue.Count;
                switch(size) {
                case 0:
                    _processing = false;
                    return;
                case 1:
                    singleMsg = _queue.Dequeue();
                    break;
                default:
                    manyMsg = new List<Message<TActor>>();
                    for(var i = 0; i < Math.Min(size, 10); i++) {
                        manyMsg.Add(_queue.Dequeue());
                    }
                    break;
                }
            }
            if(singleMsg != null) {
                ExecuteMessage(singleMsg);
                return;
            }
            foreach(var msg in manyMsg) {
                ExecuteMessage(msg);
            }
            lock(_queue) {
                if(!_queue.Any()) {
                    _processing = false;
                }
                ThreadPool.QueueUserWorkItem(Dequeue);
            }
        }

        private void ExecuteMessage(Message<TActor> msg) {
            var actor = _instance as IActor;
            actor.Scene = new Scene(_backstage,Ref,_parent,msg.Sender);
            msg.Execute(_backstage, _instance);
        }
    }
}