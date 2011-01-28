namespace Droog.Calculon {
    public interface IMailboxFactory<TRecipient> {
        IMailbox<TRecipient> CreateMailbox(MessageMeta meta);
    }
}