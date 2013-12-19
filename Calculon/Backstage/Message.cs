using System;
using System.Threading;
using System.Threading.Tasks;

namespace Droog.Calculon.Backstage {
    public class Message<TActor> {

        private readonly Func<IBackstage, TActor,Task> _message;

        public readonly ActorRef Sender;

        public Message(ActorRef sender, Func<IBackstage, TActor, Task> message) {
            Sender = sender;
            _message = message;
        }

        public Task Execute(IBackstage backstage, TActor actor) {
            return _message(backstage, actor);
        }
    }
}