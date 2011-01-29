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

    public class Stage : IDirector, IDisposable {
        private ICombinedTransport _transport;
        public ICombinedTransport Transport { get { return _transport; } }

        public void Dispose() {
            throw new NotImplementedException();
        }

        public ActorBuilder<TActor, Stage> AddActor<TActor>() {
            return new ActorBuilder<TActor, Stage>(this);
        }

        ActorBuilder<TActor, IDirector> IDirector.AddActor<TActor>() {
            return new ActorBuilder<TActor, IDirector>(this);
        }
    }

    public class ActorBuilder<TActor, TOrigin> where TOrigin : IDirector {
        protected readonly TOrigin _origin;

        public ActorBuilder(TOrigin origin) {
            _origin = origin;
            throw new NotImplementedException();
        }

        public TOrigin BuildWithMessageTransport(Func<IMessageTransport, TActor> builder) {
            return _origin;
        }
        public TOrigin BuildWithExpressionTransport(Func<IExpressionTransport, TActor> builder) {
            return _origin;
        }
        public TOrigin BuildWithCombinedTransport(Func<ICombinedTransport, TActor> builder) {
            return _origin;
        }

        public ActorBuilderWithMeta<TActor, TOrigin> WithId(string id) {
            return new ActorBuilderWithMeta<TActor, TOrigin>(_origin, id);
        }

        public TOrigin Build() {
            return _origin;
        }
    }

    public class ActorBuilderWithMeta<TActor, TOrigin> : ActorBuilder<TActor, TOrigin> where TOrigin : IDirector {
        private readonly MessageMeta _meta;

        public ActorBuilderWithMeta(TOrigin origin, string id)
            : base(origin) {
            _meta = new MessageMeta(id, typeof(TActor));
        }
        public TOrigin BuildWithMessageTransport(Func<IMessageTransport, MessageMeta, TActor> builder) {
            return _origin;
        }
        public TOrigin BuildWithExpressionTransport(Func<IExpressionTransport, MessageMeta, TActor> builder) {
            return _origin;
        }
        public TOrigin BuildWithCombinedTransport(Func<ICombinedTransport, MessageMeta, TActor> builder) {
            return _origin;
        }
    }
}
