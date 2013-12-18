using System.Threading.Tasks;

namespace Droog.Calculon {
    public interface IScene : IStage {
        Task<T> Return<T>(T value);
        TaskCompletionSource<T> GetCompletion<T>();
        void Shutdown(IActorRef actorRef);
        void Shutdown(object actor);

    }
}