using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Droog.Calculon.Messages {
    public class ShutdownMessage : IMessage{
        private readonly MessageMeta _meta;

        public ShutdownMessage(MessageMeta meta) {
            _meta = meta;
        }

        public MessageMeta Meta {get { return _meta; }}
    }
}
