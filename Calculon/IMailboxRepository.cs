namespace Droog.Calculon {
    public interface IMailboxRepository<TRecipient> {
        IMailbox<TRecipient> GetMailbox(MessageMeta meta);
    }
}
