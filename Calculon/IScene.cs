using System.Threading.Tasks;

namespace Droog.Calculon {
    public interface IScene {
        ActorRef Sender { get; }
        ActorRef Self { get; }
        ActorRef Parent { get; }
        Task<TResult> Return<TResult>(TResult value);
        Task Return();
        Completion<TResult> GetCompletion<TResult>();
        TActor CreateAndGet<TActor>(string name = null) where TActor : class;
        TActor Get<TActor>(string name) where TActor : class;
    }
}