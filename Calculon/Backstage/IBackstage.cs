namespace Droog.Calculon.Backstage {
    public interface IBackstage {
        IMailbox GetMailbox(ActorRef sender);
        IScene CreateScene(ActorRef sender);
    }
}