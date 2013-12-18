namespace Droog.Calculon.Backstage {
    public interface IBackstage {
        IMailbox GetMailbox(IActorRef sender);
    }
}