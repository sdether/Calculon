using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Droog.Calculon.Backstage;

namespace Droog.Calculon {
    public class Stage : IStage, IBackstage {

        private readonly IActorBuilder _builder;
        private readonly Dictionary<string, IMailbox> _mailboxes = new Dictionary<string, IMailbox>();

        public Stage(IActorBuilder builder) {
            _builder = builder;
        }

        public IActorRef Find(string name) {
            IMailbox actor;
            return !_mailboxes.TryGetValue(name, out actor)
                ? new ActorRef(name, typeof(object))
                : actor.Ref;
        }

        public IActorRef GetRef(object actor) {
            throw new NotImplementedException();
        }

        public TActor Create<TActor>(string name = null) where TActor : class {
            name = name ?? "__" + Guid.NewGuid();
            var mailbox = new Mailbox<TActor>(name,_builder.GetBuilder<TActor>());
            _mailboxes[mailbox.Ref.Name] = mailbox;
            return mailbox.Proxy;
        }

        T IStage.Get<T>(string address) {
            throw new NotImplementedException();
        }

        T IStage.Get<T>(IActorRef actorRef) {
            throw new NotImplementedException();
        }

        public T Get<T>(string address) {
            throw new NotImplementedException();
        }

        public T Get<T>(IActorRef actorRef) {
            throw new NotImplementedException();
        }

        public IMailbox GetMailbox(IActorRef sender) {
            return _mailboxes[sender.Name];
        }
    }
}