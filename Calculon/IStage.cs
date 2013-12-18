using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Droog.Calculon {
    public interface IStage {
        IActorRef Find(string name);
        IActorRef GetRef(object actor);
        T Create<T>(string name) where T : class;
        T Get<T>(string address) where T : class;
        T Get<T>(IActorRef actorRef) where T : class;
    }
}