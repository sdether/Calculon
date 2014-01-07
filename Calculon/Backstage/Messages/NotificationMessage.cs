using System;

namespace Droog.Calculon.Backstage.Messages {
    public class NotificationMessage : Message {
        public readonly object[] Args;

        public NotificationMessage(Guid id, string name, ActorRef sender, ActorRef receiver, object[] args)
            : base(name, sender, receiver, id) {
            Args = args;
        }
    }
}