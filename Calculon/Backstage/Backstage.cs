using System;
using System.Collections.Concurrent;

namespace Droog.Calculon.Backstage {
    public class Backstage : IBackstage {

        private interface IRoot : IActor { }
        private class Root : AActor, IRoot { }


        private readonly ConcurrentDictionary<string, IMailbox> _mailboxes = new ConcurrentDictionary<string, IMailbox>();
        private readonly IMailbox _root;
        private readonly IActorBuilder _builder;

        public Backstage() {
            _builder = new ActorBuilder();
            _root = CreateMailbox<IRoot>(null);
        }

        public ActorRef RootRef { get { return _root.Ref; } }

        public IMailbox GetMailbox(ActorRef actorRef) {
            return _mailboxes[actorRef.ToString()];
        }

        public ActorProxy<TActor> Create<TActor>(ActorRef caller, ActorRef parent, string name) where TActor : class {
            var mailbox = CreateMailbox<TActor>(parent, name);
            var senderMailbox = _mailboxes[caller.ToString()];
            return new ActorProxy<TActor>(mailbox.Ref,mailbox.BuildProxy(senderMailbox));
        }


        public ActorProxy<TActor> Find<TActor>(ActorRef caller, ActorRef actorRef) where TActor : class {
            throw new NotImplementedException();
        }

        private IMailbox<TActor> CreateMailbox<TActor>(ActorRef parent, string name = null) where TActor : class {
            name = name ?? "__" + Guid.NewGuid();
            var mailbox = new Mailbox<TActor>(parent, name, this, _builder.GetBuilder<TActor>());
            _mailboxes[mailbox.Ref.ToString()] = mailbox;
            return mailbox;
        }
    }
}