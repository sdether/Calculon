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
using Droog.Calculon.Framework;

namespace Droog.Calculon {
    public class ActorBuilderWithMeta<TActor, TOrigin> : ActorBuilder<TActor, TOrigin> {

        public ActorBuilderWithMeta(TOrigin origin, IBackstage registry, string id)
            : base(origin, registry) {
            _address = ActorAddress.Create<TActor>(id);
        }

        public TOrigin Build(Func<ITransport, ActorAddress, TActor> builder) {
            var transport = _backstage.CreateTransport(_address);
            var actor = builder(transport, _address);
            _backstage.AddActor(actor, _address, _parallelism);
            return _origin;
        }

        public new ActorBuilderWithMeta<TActor, TOrigin> WithParallelism(int parallelism) {
            _parallelism = parallelism;
            return this;
        }

    }
}