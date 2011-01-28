using System;
using System.Collections.Generic;

namespace Droog.Calculon.Framework {
    public class ImmediateDispatcher : IDispatcherLink {

        private readonly IDictionary<string, IMailbox> _addressedMailboxes = new Dictionary<string, IMailbox>();
        private readonly IDictionary<Type, IMailbox> _typedMailboxes = new Dictionary<Type, IMailbox>();

        public bool Dispatch<TData>(Message<TData> message) {
            throw new NotImplementedException();
        }

        public bool Dispatch<TRecipient>(ExpressionMessage<TRecipient> message) {
            var meta = message.Meta;
            IMailbox mbox;
            IMailbox<TRecipient> exprMbox = null;
            if(!string.IsNullOrEmpty(meta.To)) {
                lock(_addressedMailboxes) {
                    if(_addressedMailboxes.TryGetValue(meta.To, out mbox)) {
                        if(mbox.IsAlive) {
                            exprMbox = mbox as IMailbox<TRecipient>;
                        } else {
                            _addressedMailboxes.Remove(meta.To);
                        }
                    }
                }
                if(exprMbox != null) {
                    try {
                        if(exprMbox.Accept(message)) {
                            return true;
                        }
                    } catch(MailboxExpiredException) {}
                }
            }
            lock(_typedMailboxes) {
                if(_typedMailboxes.TryGetValue(meta.RecipientType, out mbox)) {
                    if(mbox.IsAlive) {
                        exprMbox = mbox as IMailbox<TRecipient>;
                    } else {
                        _typedMailboxes.Remove(meta.RecipientType);
                    }
                }
            }
            if(exprMbox != null) {
                try {
                    if(exprMbox.Accept(message)) {
                        return true;
                    }
                } catch(MailboxExpiredException) {}
            }
            return false;
        }

        public void RegisterMailbox(IMailbox mailbox, string id) {
            IMailbox current;
            lock(_addressedMailboxes) {
                if(_addressedMailboxes.TryGetValue(id, out current) && current != mailbox) {
                    current.Dispose();
                }
                _addressedMailboxes[id] = mailbox;
            }
        }

        public void RegisterMailbox(IMailbox mailbox, Type type) {
            IMailbox current;
            lock(_typedMailboxes) {
                if(_typedMailboxes.TryGetValue(type, out current) && current != mailbox) {
                    current.Dispose();
                }
                _typedMailboxes[type] = mailbox;
            }
        }

    }
}

