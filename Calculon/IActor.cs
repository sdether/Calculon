using System.Threading.Tasks;

namespace Droog.Calculon {

    public interface IActor {
        IActorContext Context { get; set; }
        ActorRef Sender { get; set; }
    }

    public abstract class AActor: IActor {

        IActorContext IActor.Context { get { return Context; } set { Context = value; } }
        ActorRef IActor.Sender { get { return Sender; } set { Sender = value; } }

        protected IActorContext Context { get; set; }
        protected ActorRef Sender { get; set; }

        protected Task<TResult> Return<TResult>(TResult value) {
            return Context.Return(value);
        }

        protected Task Return() {
            return Context.Return();
        }
    }
}