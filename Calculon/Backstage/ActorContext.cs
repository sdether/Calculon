using System;
using System.Threading.Tasks;

namespace Droog.Calculon.Backstage {
    public class ActorContext : IActorContext {
        private readonly IBackstage _backstage;
        private readonly ActorRef _self;
        private readonly ActorRef _parent;

        public ActorContext(IBackstage backstage, ActorRef self, ActorRef parent) {
            _backstage = backstage;
            _self = self;
            _parent = parent;
        }

        public ActorRef Self { get { return _self; } }
        public ActorRef Parent { get { return _parent; } }

        public Task<TResult> Return<TResult>(TResult value) {
            return TaskHelpers.GetCompletedTask(value);
        }

        public Task Return() {
            return TaskHelpers.CompletedTask;
        }

        public Completion<TResult> GetCompletion<TResult>() {
            return new Completion<TResult>();
        }

        public Completion GetCompletion() {
            return new Completion();
        }

        public ActorProxy<TActor> Create<TActor>(string name = null, Func<TActor> builder = null) where TActor : class {
            return _backstage.Create(Self, Self, name: name, builder: builder);
        }

        public ActorProxy<TActor> Find<TActor>(ActorRef actorRef) where TActor : class {
            return _backstage.Find<TActor>(Self, actorRef);
        }
    }
}