using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Droog.Calculon {
    public interface IStage {
        ActorRef GetRef(object actor);
        ActorRef Create<T>(string name) where T : class;
        T CreateAndGet<T>(string name) where T : class;
        T Get<T>(string address) where T : class;
        T Get<T>(ActorRef actorRef) where T : class;
    }
}