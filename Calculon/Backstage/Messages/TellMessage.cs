using System;

namespace Droog.Calculon.Backstage.Messages {
    public class TellMessage : Message {
        public readonly object[] Args;

        public TellMessage(string name, ActorRef sender, ActorRef receiver, object[] args)
            : base(name, sender, receiver, Guid.NewGuid()) {
            Args = args;
        }
    }
}