namespace Droog.Calculon.Framework {
    public interface IMailboxRepository<TRecipient> {
        IMailbox<TRecipient> GetMailbox(MessageMeta meta);
    }
}
