namespace Droog.Calculon.Backstage {
    public interface IBackstage {
        ActorRef RootRef { get; }
        IMailbox GetMailbox(ActorRef actorRef);
        TActor CreateAndGet<TActor>(ActorRef caller, ActorRef parent, string name = null) where TActor : class;
        TActor Get<TActor>(ActorRef caller, ActorRef actorRef) where TActor : class;
    }
}