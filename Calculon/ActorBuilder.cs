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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Droog.Calculon.Framework;

namespace Droog.Calculon {
    public class ActorBuilder<TActor, TOrigin> {
        protected readonly TOrigin _origin;
        protected readonly IBackstage _backstage;
        protected ActorAddress _address;
        protected int _parallelism = 1;

        public ActorBuilder(TOrigin origin, IBackstage backstage) {
            _origin = origin;
            _backstage = backstage;
            _address = ActorAddress.Create<TActor>();
        }

        public TOrigin BuildWithMessageTransport(Func<IMessageTransport, TActor> builder) {
            var transport = _backstage.CreateMessageTransport(_address);
            var actor = builder(transport);
            _backstage.AddActor(actor,_address,_parallelism);
            return _origin;
        }

        public TOrigin BuildWithExpressionTransport(Func<IExpressionTransport, TActor> builder) {
            var transport = _backstage.CreateExpressionTransport(_address);
            var actor = builder(transport);
            _backstage.AddActor(actor, _address, _parallelism);
            return _origin;
        }

        public TOrigin BuildWithCombinedTransport(Func<ICombinedTransport, TActor> builder) {
            var transport = _backstage.CreateCombinedTransport(_address);
            var actor = builder(transport);
            _backstage.AddActor(actor, _address, _parallelism);
            return _origin;
        }

        public ActorBuilder<TActor, TOrigin> WithParallelism(int parallelism) {
            _parallelism = parallelism;
            return this;
        }

        public ActorBuilderWithMeta<TActor, TOrigin> WithId(string id) {
            return new ActorBuilderWithMeta<TActor, TOrigin>(_origin, _backstage, id);
        }

        public TOrigin Build() {
            var actorType = typeof(TActor);
            var bestCtor = (from ctor in actorType.GetConstructors()
                            let parameters = ctor.GetParameters()
                            where CanBuild(parameters)
                            orderby parameters.Length descending
                            select ctor).FirstOrDefault();
            if(bestCtor == null) {
                throw new ActorBuilderNoCtorException(actorType);
            }
            var ctorParameters = bestCtor.GetParameters();
            var args = new object[ctorParameters.Length];
            for(var i = 0; i < ctorParameters.Length; i++) {
                var pType = ctorParameters[i].ParameterType;
                if(pType == typeof(IExpressionTransport)) {
                    args[i] = _backstage.CreateExpressionTransport(_address);
                } else if(pType == typeof(IMessageTransport)) {
                    args[i] = _backstage.CreateMessageTransport(_address);
                } else if(pType == typeof(ICombinedTransport)) {
                    args[i] = _backstage.CreateCombinedTransport(_address);
                } else if(pType == typeof(ActorAddress)) {
                    args[i] = _address;
                }
            }
            var actor = (TActor)Activator.CreateInstance(actorType, args);
            var mailbox = new Mailbox<TActor>(_address, actor, _parallelism);
            _backstage.AddActor(actor, _address, _parallelism);
            return _origin;
        }

        private static bool CanBuild(IEnumerable<ParameterInfo> parameters) {
            return !(from p in parameters
                     where typeof(IExpressionTransport) != p.ParameterType
                        && typeof(IMessageTransport) != p.ParameterType
                        && typeof(ICombinedTransport) != p.ParameterType
                        && typeof(ActorAddress) != p.ParameterType
                     select p).Any();
        }
    }

    public class ActorBuilderNoCtorException : Exception {
        public readonly Type ActorType;

        public ActorBuilderNoCtorException(Type actorType) {
            ActorType = actorType;
        }
    }
}