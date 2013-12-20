namespace Droog.Calculon.Backstage {
    public interface IBackstage {
        ActorRef RootRef { get; }
        IMailbox GetMailbox(ActorRef actorRef);
        ActorProxy<TActor> Create<TActor>(ActorRef caller, ActorRef parent, string name = null) where TActor : class;
        ActorProxy<TActor> Find<TActor>(ActorRef caller, ActorRef actorRef) where TActor : class;
    }
}