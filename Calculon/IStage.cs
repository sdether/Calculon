namespace Droog.Calculon {
    public interface IStage {
        ActorProxy<TActor> Create<TActor>(string name = null) where TActor : class;
        ActorProxy<TActor> Find<TActor>(ActorRef actorRef) where TActor : class;
    }

    
}