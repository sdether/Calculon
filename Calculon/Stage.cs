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
using Droog.Calculon.Messages;

namespace Droog.Calculon {

    public class Stage : IDirector, IDisposable {
        private readonly ActorAddress _address;
        private readonly ITransport _transport;
        private readonly ImmediateDispatchBackstage _backstage;


        public Stage() {
            _address = ActorAddress.Create(GetType());
            _backstage = new ImmediateDispatchBackstage();
            _backstage.AddActor<IDirector>(this, _address, 100);
            _transport = _backstage.CreateTransport(_address);
        }
        public ITransport Transport { get { return _transport; } }

        public ActorBuilder<TActor, Stage> AddActor<TActor>() {
            return new ActorBuilder<TActor, Stage>(this, _backstage);
        }

        void IDirector.RetireActor(ActorAddress address, MessageMeta meta) {
            _backstage.Dispatch(new ShutdownMessage(meta.Sender, address));
        }

        ActorBuilder<TActor, IDirector> IDirector.AddActor<TActor>() {
            return new ActorBuilder<TActor, IDirector>(this, _backstage);
        }

        public void Dispose() {
        }

    }
}
