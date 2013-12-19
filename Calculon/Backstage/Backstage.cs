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
            return _mailboxes[actorRef.Name];
        }

        public TActor CreateAndGet<TActor>(ActorRef caller, ActorRef parent, string name = null) where TActor : class {
            var mailbox = CreateMailbox<TActor>(parent, name);
            var senderMailbox = _mailboxes[caller.Name];
            return mailbox.BuildProxy(senderMailbox);
        }

        public TActor Get<TActor>(ActorRef caller, ActorRef actorRef) where TActor : class {
            var mailbox = _mailboxes[actorRef.Name].As<TActor>();
            var senderMailbox = _mailboxes[caller.Name];
            return mailbox.BuildProxy(senderMailbox);
        }

        private IMailbox<TActor> CreateMailbox<TActor>(ActorRef parent, string name = null) where TActor : class {
            name = name ?? "__" + Guid.NewGuid();
            var mailbox = new Mailbox<TActor>(parent, name, this, _builder.GetBuilder<TActor>());
            _mailboxes[mailbox.Ref.Name] = mailbox;
            return mailbox;
        }
    }
}