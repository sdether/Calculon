/*
 * Calculon 
 * Copyright (C) 2011 Arne F. Claassen
 * http://www.claassen.net/geek/blog geekblog [at] claassen [dot] net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
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
            if(!string.IsNullOrEmpty(meta.Recipient.Id)) {
                lock(_addressedMailboxes) {
                    if(_addressedMailboxes.TryGetValue(meta.Recipient.Id, out mbox)) {
                        if(mbox.IsAlive) {
                            exprMbox = mbox as IMailbox<TRecipient>;
                        } else {
                            _addressedMailboxes.Remove(meta.Recipient.Id);
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
                if(_typedMailboxes.TryGetValue(meta.Recipient.Type, out mbox)) {
                    if(mbox.IsAlive) {
                        exprMbox = mbox as IMailbox<TRecipient>;
                    } else {
                        _typedMailboxes.Remove(meta.Recipient.Type);
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

