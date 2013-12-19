using System;
using System.Threading.Tasks;

namespace Droog.Calculon.Backstage {
    public class Scene : IScene {
        private readonly IBackstage _backstage;
        private readonly ActorRef _self;
        private readonly ActorRef _parent;
        private readonly ActorRef _sender;

        public Scene(IBackstage backstage, ActorRef self, ActorRef parent, ActorRef sender) {
            _backstage = backstage;
            _self = self;
            _parent = parent;
            _sender = sender;
        }

        public ActorRef Sender { get { return _sender; } }
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

        public TActor CreateAndGet<TActor>(string name = null) where TActor : class {
            return _backstage.CreateAndGet<TActor>(Self, Self, name);
        }

        public TActor Get<TActor>(string name) where TActor : class {
            return _backstage.Get<TActor>(Self, new ActorRef(name, typeof(TActor)));
        }

    }
}