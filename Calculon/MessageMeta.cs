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

    public class ActorAddress {

        public static ActorAddress Create<T>() {
            return Create(typeof(T));
        }

        public static ActorAddress Create<T>(string id) {
            return Create(id, typeof(T));
        }

        public static ActorAddress Create(Type type) {
            return new ActorAddress("__" + Guid.NewGuid(), type, true);
        }

        public static ActorAddress Create(string id) {
            return new ActorAddress(id, null, false);
        }

        public static ActorAddress Create(string id, Type type) {
            return new ActorAddress(id, type, false);
        }
 
        public readonly string Id;
        public readonly Type Type;
        public readonly bool IsAnonymous;
        public readonly bool IsUntyped;

        private ActorAddress(string id, Type type, bool isAnonymous) {
            Id = id;
            Type = type;
            IsAnonymous = isAnonymous;
            IsUntyped = Type == null;
        }

        public MessageMeta To<T>() {
            return new MessageMeta(this, Create<T>());
        }

        public MessageMeta To<T>(string id) {
            return new MessageMeta(this, Create<T>(id));
        }

        public MessageMeta To(string id) {
            return new MessageMeta(this, Create(id));
        }

        public MessageMeta To(string id, Type type) {
            return new MessageMeta(this, Create(id, type));
        }

        public MessageMeta To(Type type) {
            return new MessageMeta(this, Create(type));
        }

        public MessageMeta To(ActorAddress address) {
            return new MessageMeta(this, address);
        }

        public MessageMeta ToUnknown() {
            return new MessageMeta(this, new ActorAddress(null, null, true));
        }
   }

    public class MessageMeta {
        public readonly ActorAddress Sender;
        public readonly ActorAddress Recipient;

        public MessageMeta(ActorAddress sender, ActorAddress recipient) {
            Sender = sender;
            Recipient = recipient;
        }

        public MessageMeta Reply() {
            return new MessageMeta(Recipient, Sender);
        }
    }
}