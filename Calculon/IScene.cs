using System.Threading.Tasks;

namespace Droog.Calculon {
    public interface IScene {
        ActorRef Sender { get; }
        Task<TResult> Return<TResult>(TResult value);
        Completion<TResult> GetCompletion<TResult>();
        void Shutdown(ActorRef actorRef);
        void Shutdown(object actor);
    }
}