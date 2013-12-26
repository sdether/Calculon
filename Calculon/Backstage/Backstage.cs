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

namespace Droog.Calculon.Backstage {
    public class Backstage : IBackstage {

        private interface IRoot : IActor { }
        private class Root : AActor, IRoot { }

        private readonly ConcurrentDictionary<ActorRef, IParentMailbox> _mailboxes = new ConcurrentDictionary<ActorRef, IParentMailbox>();
        private readonly IMailbox _root;
        private readonly IActorBuilder _builder;

        public Backstage() {
            _builder = new ActorBuilder();
            _root = CreateMailbox<IRoot>(null);
        }

        public ActorRef RootRef { get { return _root.Ref; } }

        public IMailbox GetMailbox(ActorRef actorRef) {
            return _mailboxes[actorRef];
        }

        public ActorProxy<TActor> Create<TActor>(ActorRef caller, ActorRef parent, string name = null, Func<TActor> builder = null) where TActor : class {
            var mailbox = CreateMailbox(parent, name, builder);
            var callerMailbox = GetMailbox(caller);
            return new ActorProxy<TActor>(mailbox.Ref, mailbox.BuildProxy(callerMailbox));
        }

        public ActorProxy<TActor> Find<TActor>(ActorRef caller, ActorRef actorRef) where TActor : class {
            var mailbox = GetMailbox(actorRef).As<TActor>();
            var callerMailbox = GetMailbox(caller);
            return new ActorProxy<TActor>(mailbox.Ref, mailbox.BuildProxy(callerMailbox));
        }

        private IMailbox<TActor> CreateMailbox<TActor>(ActorRef parent, string name = null, Func<TActor> builder = null) where TActor : class {
            name = name ?? Guid.NewGuid().ToString();
            var parentMailbox = _mailboxes[parent];
            var mailbox = new Mailbox<TActor>(parentMailbox, name, this, builder ?? _builder.GetBuilder<TActor>());
            _mailboxes[mailbox.Ref] = mailbox;
            return mailbox;
        }
    }
}