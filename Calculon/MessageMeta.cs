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
    public class MessageMeta {
        public readonly ActorAddress Sender;
        public readonly ActorAddress Recipient;
        public readonly Type MessageType;

        public MessageMeta(Type type, ActorAddress sender, ActorAddress recipient) {
            MessageType = type;
            Sender = sender;
            Recipient = recipient;
        }

        public MessageMeta Reply<TMessage>() {
            return new MessageMeta(typeof(TMessage), Recipient, Sender);
        }
    }
}