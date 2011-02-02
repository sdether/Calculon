using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Droog.Calculon.Messages {
    public class ShutdownMessage : IMessage {
        private readonly MessageMeta _meta;

        public ShutdownMessage(ActorAddress sender, ActorAddress recipient) {
            _meta = new MessageMeta(typeof(ShutdownMessage), sender, recipient);
        }

        public MessageMeta Meta { get { return _meta; } }
        public void Undeliverable() { }
    }
}
