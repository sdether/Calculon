/*
 * Calculon 
 * Copyright (C) 2011 Arne F. Claassen
 * http://www.claassen.net/geek/blog geekblog [at] claassen [dot] net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;

namespace Droog.Calculon {

    public interface IMessage {
        MessageMeta Meta { get; }
    }
    public class MessageMeta {
        private string _from;
        private string _to;
        private Type _recipient;
        private Type _sender;

        public MessageMeta(string from, Type sender) {
            _from = from;
            _sender = sender;
        }

        private MessageMeta() {}

        public string From { get { return _from; } }
        public string To { get { return _to; } }
        public Type RecipientType { get { return _recipient; } }
        public Type SenderType { get { return _sender; } }

        public MessageMeta For(string recipientId) {
            return For(recipientId, null);
        }

        public MessageMeta For<TRecipient>() {
            return For(null, typeof(TRecipient));
        }

        public MessageMeta For<TRecipient>(string recipientId) {
            return For(recipientId, typeof(TRecipient));
        }

        public MessageMeta For(string recipientId, Type recipientType) {
            return new MessageMeta { _from = _from, _sender = _sender, _to = recipientId, _recipient = recipientType };
        }

        public MessageMeta Reply() {
            return new MessageMeta { _from = _to, _sender = _recipient, _to = _from, _recipient = _sender };
        }
    }
}