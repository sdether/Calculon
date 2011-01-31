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
using System.Linq;

namespace Droog.Calculon.Framework {

    /// <summary>
    /// Test implementation handling both backstage and dispatcher in one. This has lock contentions around the mailboxes.
    /// Backstage should contain an instance of DispatchChain instead and hand off new mailboxes to a chain of IMailboxRepositories
    /// which are in turn tied to the IDispatchLinks used by the DispatchChain
    /// </summary>
    public class ImmediateDispatchBackstage : IBackstage, IDispatcher {

        private readonly IDictionary<string, IMailbox> _mailboxes = new Dictionary<string, IMailbox>();

        public void AddActor<TActor>(TActor actor, ActorAddress address, int parallelism) {
            var mailbox = new Mailbox<TActor>(address, actor, parallelism);
            lock(_mailboxes) {
                _mailboxes[mailbox.Recipient.Id] = mailbox;
            }
        }

        public IExpressionTransport CreateExpressionTransport(ActorAddress address) {
            return new ExpressionTransport(address, this);
        }

        public IMessageTransport CreateMessageTransport(ActorAddress address) {
            return new MessageTransport(address, this);
        }

        public ICombinedTransport CreateCombinedTransport(ActorAddress address) {
            return new CombinedTransport(address, this);
        }

        public void Dispatch(IMessage message) {
            var meta = message.Meta;
            IMailbox mbox;

            // try to dispatch by id
            if(!string.IsNullOrEmpty(meta.Recipient.Id)) {
                lock(_mailboxes) {
                    if(_mailboxes.TryGetValue(meta.Recipient.Id, out mbox)) {
                        if(!mbox.IsAlive) {
                            _mailboxes.Remove(meta.Recipient.Id);
                        }
                    }
                }
                if(mbox.IsAlive) {
                    try {
                        if(mbox.Accept(message)) {
                            return;
                        }
                    } catch(MailboxExpiredException) { }
                }
            }

            // IMessage doesn't have response handle, so a failure is swallowed
        }

        public void Dispatch<TData>(Message<TData> message) {
            throw new NotImplementedException();
        }

        public void Dispatch<TRecipient>(ExpressionMessage<TRecipient> message) {
            var meta = message.Meta;
            IMailbox mbox;
            IMailbox<TRecipient> exprMbox = null;

            // try to dispatch by id
            if(!string.IsNullOrEmpty(meta.Recipient.Id)) {
                lock(_mailboxes) {
                    if(_mailboxes.TryGetValue(meta.Recipient.Id, out mbox)) {
                        if(mbox.IsAlive) {
                            exprMbox = mbox as IMailbox<TRecipient>;
                        } else {
                            _mailboxes.Remove(meta.Recipient.Id);
                        }
                    }
                }
                if(exprMbox != null) {
                    try {
                        if(exprMbox.Accept(message)) {
                            return;
                        }
                    } catch(MailboxExpiredException) { }
                }
            }

            // find anonymous recipient mailboxes
            IMailbox<TRecipient>[] candidates;
            lock(_mailboxes) {
                candidates = (from candidateMbox in _mailboxes.Values
                              let candidateExprMbox = candidateMbox as IMailbox<TRecipient>
                              where candidateExprMbox != null && candidateExprMbox.Recipient.IsAnonymous
                              select candidateExprMbox).ToArray();
                foreach(var dead in candidates.Where(x => !x.IsAlive)) {
                    dead.Dispose();
                    _mailboxes.Remove(dead.Recipient.Id);
                }
            }

            // see if any of the candidaes will dispatch the message
            if(candidates.Where(x => x.IsAlive).Any(candidate => candidate.Accept(message))) {
                return;
            }

            // couldn't do anything with the message, throw
            message.Throw(new NoSuchRecipientException(message.Meta));
        }
    }
}