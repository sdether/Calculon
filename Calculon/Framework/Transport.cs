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

namespace Droog.Calculon.Framework {
    public class Transport : ITransport {
        private readonly ActorAddress _address;
        private readonly IDispatcher _dispatcher;

        public Transport(ActorAddress address, IDispatcher dispatcher) {
            _address = address;
            _dispatcher = dispatcher;
        }

        public ActorAddress Sender { get { return _address; } }

        public void Send(IMessage message) {
            _dispatcher.Dispatch(message);
        }
    }
}