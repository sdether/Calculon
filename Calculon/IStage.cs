namespace Droog.Calculon {
    public interface IStage {
        T CreateAndGet<T>(string name) where T : class;
        T Get<T>(string name) where T : class;
        T Get<T>(ActorRef actorRef) where T : class;
    }
}