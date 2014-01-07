using System;

namespace Droog.Calculon.Backstage.Messages {
    public class AskMessage : Message {
        public readonly object[] Args;

        public AskMessage(Guid id, string name, ActorRef sender, ActorRef receiver, object[] args)
            : base(name, sender, receiver, id) {
            Args = args;
        }
    }
}