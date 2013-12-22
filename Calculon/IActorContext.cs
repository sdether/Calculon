using System;
using System.Threading.Tasks;

namespace Droog.Calculon {
    public interface IActorContext {
        ActorRef Self { get; }
        ActorRef Parent { get; }
        Task<TResult> Return<TResult>(TResult value);
        Task Return();
        Completion<TResult> GetCompletion<TResult>();
        Completion GetCompletion();
        ActorProxy<TActor> Create<TActor>(string name = null, Func<TActor> builder = null) where TActor : class;
        ActorProxy<TActor> Find<TActor>(ActorRef actorRef) where TActor : class;
    }
}