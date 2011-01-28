using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Droog.Calculon.Framework {
    public class MailboxRepository<TRecipient> : IMailboxRepository<TRecipient> {
        private readonly IMailboxFactory<TRecipient> _factory;
        private readonly Dictionary<string, IMailbox<TRecipient>> _repository = new Dictionary<string, IMailbox<TRecipient>>();

        public MailboxRepository(IMailboxFactory<TRecipient> factory) {
            _factory = factory;
        }

        public IMailbox<TRecipient> GetMailbox(MessageMeta meta) {
            IMailbox<TRecipient> mbox;
            lock(_repository) {
                _repository.TryGetValue(meta.To, out mbox);
                if(mbox != null && !mbox.IsAlive) {
                    _repository.Remove(meta.To);
                    mbox = null;
                }
                if(mbox == null) {
                    mbox = _factory.CreateMailbox(meta);
                    _repository[meta.To] = mbox;
                }
            }
            return mbox;
        }
    }
}
