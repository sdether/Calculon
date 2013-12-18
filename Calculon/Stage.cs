using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Droog.Calculon.Backstage;

namespace Droog.Calculon {
    public class Stage : IStage, IBackstage {
        readonly Dictionary<string, Mailbox> _mailboxes = new Dictionary<string, Mailbox>();

        public IActorRef Find(string address) {
            Mailbox actor;
            return !_mailboxes.TryGetValue(address, out actor)
                ? new ActorRef(this, address, typeof(Void))
                : actor.Ref;
        }

        public IActorRef GetRef(object actor) {
            throw new NotImplementedException();
        }

        public T Create<T>(string address) where T : class {
            var actor = _mailboxes[address] = new Actor {
                Ref = new ActorRef(this, address, typeof(T)),
                Instance = Activator.CreateInstance<T>() as IActor,
                Mailbox = new Mailbox()
            };
            return actor.Proxy as T;
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

        public Task<TResult> Send<TActor, TResult>(string address, Expression<Func<TActor, Task<TResult>>> expression) {
            throw new NotImplementedException();
        }

        public Mailbox GetMailbox(IActorRef sender) {
            throw new NotImplementedException();
        }
    }
}