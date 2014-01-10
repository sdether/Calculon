/* ----------------------------------------------------------------------------
 * Copyright (C) 2013 Arne F. Claassen
 * geekblog [at] claassen [dot] net
 * http://github.com/sdether/Calculon
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 * ----------------------------------------------------------------------------
 */

using System;
using System.Collections.Concurrent;
using Castle.DynamicProxy;
using Droog.Calculon.Backstage.Messages;
using Droog.Calculon.Backstage.SystemActors;

namespace Droog.Calculon.Backstage {
    public class Backstage : IBackstage, IMailbox {

        private class MailboxProxy : IMessageReceiver {
            private readonly Backstage _backstage;
            private IMailbox _proxyTarget;

            public MailboxProxy(Backstage backstage, IMailbox proxyTarget) {
                _backstage = backstage;
                _proxyTarget = proxyTarget;
            }

            public ActorRef Ref { get { return _proxyTarget.Ref; } }

            public void Enqueue(Message msg) {
                if(!VerifyMailbox()) {
                    return;
                }
                while(true) {
                    try {
                        _proxyTarget.Enqueue(msg);
                        break;
                    } catch(DeadMailboxException) {
                        if(!VerifyMailbox())
                            break;
                    }
                }
            }

            private bool VerifyMailbox() {
                if(_proxyTarget == null) {
                    _proxyTarget = _backstage.GetMailbox(Ref);
                    if(_proxyTarget == null) {
                        // TODO: route to dead letters
                        return false;
                    }
                }
                return true;
            }
        }
        private static readonly ProxyGenerator PROXY_GENERATOR = new ProxyGenerator();

        private interface IRoot : IActor { }
        private class Root : AActor, IRoot { }

        private readonly ConcurrentDictionary<ActorRef, IMailbox> _mailboxes = new ConcurrentDictionary<ActorRef, IMailbox>();
        private readonly IActorBuilder _builder;
        private readonly ActorRef _ref = ActorRef.Parse("/");
        public Backstage() {
            _builder = new ActorBuilder();
            _mailboxes[_ref] = this;
            CreateMailbox<Cast>(_ref, "cast");
            CreateMailbox<Stagehands>(_ref, "stagehands");
            CreateMailbox<DeadLetters>(_ref, "deadletters");
            CreateMailbox<Temp>(_ref, "temp");
        }

        private IMailbox GetMailbox(ActorRef actorRef) {
            return _mailboxes[actorRef];
        }

        public ActorProxy<TActor> Create<TActor>(ActorRef caller, ActorRef parent, string name = null, Func<TActor> builder = null) where TActor : class, IActor {
            var mailbox = CreateMailbox(parent, name, builder);
            var targetRef = mailbox.Ref;
            return BuildActorProxy<TActor>(caller, targetRef);
        }

        private ActorProxy<TActor> BuildActorProxy<TActor>(ActorRef caller, ActorRef targetRef) where TActor : class, IActor {
            var callerMailbox = GetMailbox(caller);
            var targetMailbox = GetMailbox(targetRef);
            // TODO: handle lack of target mailbox by routing to dead letters
            return new ActorProxy<TActor>(
                targetRef,
                PROXY_GENERATOR.CreateInterfaceProxyWithoutTarget<TActor>(
                    new ActorProxyInterceptor(callerMailbox, new MailboxProxy(this, targetMailbox))
                )
            );
        }

        public ActorProxy<TActor> Find<TActor>(ActorRef caller, ActorRef actorRef) where TActor : class, IActor {
            return BuildActorProxy<TActor>(caller, actorRef);
        }

        ActorRef IMessageReceiver.Ref {
            get { throw new NotImplementedException(); }
        }

        void IMessageReceiver.Enqueue(Message message) {
            var created = message as CreatedMessage;
            if(created != null) {
                return;
            }
            throw new InvalidOperationException("root does not accept messages of type: " + message.GetType());
        }

        public void Enqueue(Message message) {
            var mailbox = GetMailbox(message.Receiver);

            // TODO: if no mailbox found, stuff in dead letter
            mailbox.Enqueue(message);
        }

        private IMailbox CreateMailbox<TActor>(ActorRef parent, string name = null, Func<TActor> builder = null) where TActor : class, IActor {
            name = name ?? Guid.NewGuid().ToString();
            var mailbox = new Mailbox<TActor>(parent, name, this, builder ?? _builder.GetBuilder<TActor>());
            _mailboxes[mailbox.Ref] = mailbox;
            return mailbox;
        }

        bool IMailbox.IsMailboxFor<TActor>() {
            throw new NotImplementedException();
        }

        MessageResponse IMailbox.CreatePendingResponse(Type type) {
            throw new NotImplementedException();
        }
    }
}