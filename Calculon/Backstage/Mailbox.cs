using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Droog.Calculon.Backstage {
    public interface IMailbox {
        IActorRef Ref { get; }
        void EnqueueResponseMessage<TResult>(Guid id, TResult result);
        bool IsMailboxFor<TActor>();
        IMailbox<TActor> As<TActor>() where TActor : class;
    }

    public interface IMailbox<TActor> : IMailbox where TActor: class {
        TActor Proxy { get; }
        void EnqueueExpression<TResult>(Guid id, IActorRef sender, Expression<Func<TActor, Task<TResult>>> expr);
        void EnqueueExpression<TResult>(Guid id, IActorRef sender, Expression<Func<TActor, Task>> expr);
        void EnqueueExpression<TResult>(Guid id, IActorRef sender, Expression<Action<TActor>> expr);
        void SetInstance(TActor instance);
    }

    public class Mailbox<TActor> : IInterceptor, IMailbox<TActor> where TActor : class {

        private static ProxyGenerator _generator = new ProxyGenerator();
        private readonly IActorRef _actorRef;
        private readonly Queue<Message<TActor>> _queue = new Queue<Message<TActor>>();
        private readonly Dictionary<Guid,object> _pendingResponses = new Dictionary<Guid, object>();
        private readonly TActor _proxy;
        private TActor _instance;

        public Mailbox(string name) {
            _actorRef = new ActorRef(name);
            _proxy = _generator.CreateInterfaceProxyWithoutTarget<TActor>(this);
        }

        public IActorRef Ref { get { return _actorRef; } }
        public TActor Proxy { get { return _proxy; } }

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

        public void EnqueueExpression<TResult>(Guid id, IActorRef sender, Expression<Func<TActor, Task<TResult>>> expr) {
            _queue.Enqueue(Message<TActor>.FromExpression(id, sender, expr));
        }

        public void EnqueueExpression<TResult>(Guid id, IActorRef sender, Expression<Func<TActor, Task>> expr) {
            _queue.Enqueue(Message<TActor>.FromExpression(id, sender, expr));
        }

        public void EnqueueExpression<TResult>(Guid id, IActorRef sender, Expression<Action<TActor>> expr) {
            _queue.Enqueue(Message<TActor>.FromExpression(id, sender, expr));
        }

        public void SetInstance(TActor instance) {
            _instance = instance;
        }

        public void Intercept(IInvocation invocation) {
            throw new NotImplementedException();
        }
    }
}