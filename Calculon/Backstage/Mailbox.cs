using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Droog.Calculon.Backstage {
    public class Mailbox<TActor> : IMailbox<TActor> where TActor : class {

        private static ProxyGenerator _generator = new ProxyGenerator();

        private readonly Func<TActor> _builder;
        private readonly IActorRef _actorRef;
        private readonly Queue<Message<TActor>> _queue = new Queue<Message<TActor>>();
        private readonly Dictionary<Guid,object> _pendingResponses = new Dictionary<Guid, object>();
        private TActor _instance;

        public Mailbox(string name, Func<TActor> builder) {
            _builder = builder;
            _actorRef = new ActorRef(name, typeof(TActor));
            _instance = _builder();
        }

        public IActorRef Ref { get { return _actorRef; } }

        public void EnqueueResponseMessage<TResult>(Guid id, TResult result) {
            object completionObject;
            if(!_pendingResponses.TryGetValue(id, out completionObject)) {
                return;
            }
            var completion = completionObject as TaskCompletionSource<TResult>;
            if(completion == null) {
                return;
            }
            _queue.Enqueue(Message<TActor>.FromResponse(result,completion));
        }

        public bool IsMailboxFor<TActor1>() {
            return typeof(TActor).IsAssignableFrom(typeof(TActor1));
        }

        public IMailbox<TActor1> As<TActor1>() where TActor1 : class {
            return this as IMailbox<TActor1>;
        }

        public MessageResponse<TResult> CreatePendingResponse<TResult>() {
            var tcs = new TaskCompletionSource<TResult>();
            var response = new MessageResponse<TResult>(tcs);
            _pendingResponses.Add(response.Id,tcs);
            return response;
        }

        public TActor Proxy { get; private set; }

        public void EnqueueExpression<TResult>(Guid id, IActorRef sender, Func<TActor, Task<TResult>> expr) {
            _queue.Enqueue(Message<TActor>.FromExpression(id, sender, expr));
        }

        public void EnqueueExpression(Guid id, IActorRef sender, Func<TActor, Task> expr) {
            _queue.Enqueue(Message<TActor>.FromExpression(id, sender, expr));
        }

        public void EnqueueExpression(Guid id, IActorRef sender, Action<TActor> expr) {
            _queue.Enqueue(Message<TActor>.FromExpression(id, sender, expr));
        }

        public void SetInstance(TActor instance) {
            _instance = instance;
        }

        public TActor BuildProxy(IMailbox sender) {
            return _generator.CreateInterfaceProxyWithoutTarget<TActor>(new ActorProxy<TActor>(sender, this));
        }

    }
}