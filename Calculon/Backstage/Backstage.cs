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
            _root = CreateMailbox<IRoot>();
        }
        public IMailbox GetMailbox(ActorRef sender) {
            throw new NotImplementedException();
        }

        public IScene CreateScene(ActorRef sender) {
            throw new NotImplementedException();
        }
        IMailbox IBackstage.GetMailbox(ActorRef sender) {
            return _mailboxes[sender.Name];
        }

        public IMailbox<TActor> CreateMailbox<TActor>(string name = null) where TActor : class {
            name = name ?? "__" + Guid.NewGuid();
            var mailbox = new Mailbox<TActor>(_root.Ref, name, this, _builder.GetBuilder<TActor>());
            _mailboxes[mailbox.Ref.Name] = mailbox;
            return mailbox;
        }
    }
}