using System;

namespace Droog.Calculon {
    public class Stage : IStage {

        private readonly Backstage.Backstage _backstage;

        public Stage() {
            _backstage = new Backstage.Backstage();
        }


        public ActorRef GetRef(object actor) {
            throw new NotImplementedException();
        }

        public ActorRef Create<TActor>(string name) where TActor : class {
            var mailbox = _backstage.CreateMailbox<TActor>(name);
            return mailbox.Ref;
        }

        public TActor CreateAndGet<TActor>(string name = null) where TActor : class {
            var mailbox = _backstage.CreateMailbox<TActor>(name);
            return mailbox.BuildProxy(_root);
        }

        public TActor Get<TActor>(string address) where TActor : class {
            var mailbox = _mailboxes[address].As<TActor>();
            return mailbox.BuildProxy(_root);
        }

        public TActor Get<TActor>(ActorRef actorRef) where TActor : class {
            throw new NotImplementedException();
        }


    }
}