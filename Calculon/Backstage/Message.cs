using System;
using System.Threading.Tasks;

namespace Droog.Calculon.Backstage {
    public class Message<TActor> {

        private readonly Action<IBackstage, TActor> _action;

        public static Message<TActor> FromExpression<TResult>(Guid id, IActorRef sender, Func<TActor, Task<TResult>> expr) {
            return new Message<TActor>((backstage, actor) => {
                var r = expr(actor);
                var mailbox = backstage.GetMailbox(sender);
                mailbox.EnqueueResponseMessage(id, r.Result);
            });
        }

        public static Message<TActor> FromExpression(Guid id, IActorRef sender, Func<TActor, Task> expr) {
            return new Message<TActor>((backstage, actor) => {
                var r = expr(actor);
                var mailbox = backstage.GetMailbox(sender);
                mailbox.EnqueueResponseMessage(id, 0);
            });
        }

        public static Message<TActor> FromExpression(Guid id, IActorRef sender, Action<TActor> expr) {
            return new Message<TActor>((backstage, actor) => expr(actor));
        }

        public static Message<TActor> FromResponse<TResult>(TResult result, TaskCompletionSource<TResult> completionSource) {
            return new Message<TActor>((backstage, actor) => completionSource.SetResult(result));
        }

        private Message(Action<IBackstage, TActor> action) {
            _action = action;
        }

        public void Execute(IBackstage backstage, TActor actor) {
            _action(backstage, actor);
        }
    }
}