namespace Droog.Calculon.Backstage {
    public interface IBackstage {
        Mailbox GetMailbox(IActorRef sender);
    }
}