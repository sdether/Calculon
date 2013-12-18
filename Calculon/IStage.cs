using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Droog.Calculon {
    public interface IStage {
        IActorRef Find(string address);
        IActorRef GetRef(object actor);
        T Create<T>(string address) where T : class;
        T Get<T>(string address) where T : class;
        T Get<T>(IActorRef actorRef) where T : class;
        Task<TResult> Send<TActor, TResult>(string address, Expression<Func<TActor, Task<TResult>>> expression);
    }
}