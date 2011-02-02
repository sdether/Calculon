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
    public class Message<TData> : IMessage {
        public Message(TData value, ActorAddress sender, ActorAddress recipient) {
            Value = value;
            Meta = new MessageMeta(typeof(Message<TData>), sender, recipient);
        }

        public Message(TData value, ActorAddress sender, ActorAddress recipient, Type responseType) {
            Value = value;
            Meta = new MessageMeta(typeof(Message<TData>), sender, recipient);
            ExpectedResponse = responseType;
        }

        public TData Value { get; private set; }
        public MessageMeta Meta { get; private set; }
        public Type ExpectedResponse { get; private set; }
        public bool ExpectsResponse { get { return ExpectedResponse != null; } }

        public void Undeliverable() { }
    }
}