using System.Threading.Tasks;

namespace Droog.Calculon {

    public interface IActor {
        IActorContext Context { get; set; }
        ActorRef Sender { get; set; }
    }

    public abstract class AActor: IActor {
        public IActorContext Context { get; set; }
        public ActorRef Sender { get; set; }

        public Task<TResult> Return<TResult>(TResult value) {
            return Context.Return(value);
        }
        public Task Return() {
            return Context.Return();
        }
    }
}