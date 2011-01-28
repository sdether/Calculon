using System;

namespace Droog.Calculon.Framework {
    public class RepositoryDispatcher<TRecipient> : IDispatcherLink {
        private readonly IMailboxRepository<TRecipient> _repository;

        public RepositoryDispatcher(IMailboxRepository<TRecipient> repository) {
            _repository = repository;
        }

        public bool Dispatch<TData>(Message<TData> message) {
            return false;
        }

        public bool Dispatch<TRecipientX>(ExpressionMessage<TRecipientX> message) {
            var castMessage = message as ExpressionMessage<TRecipient>;
            if(castMessage == null) {
                return false;
            }
            var mbox = _repository.GetMailbox(message.Meta);
            return mbox.Accept(castMessage);
        }
    }
}