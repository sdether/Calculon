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
using System.Linq.Expressions;
using MindTouch.Tasking;

namespace Droog.Calculon.Framework {
    public class AddressedExpressionTransport<TRecipient> : IAddressedExpressionTransport<TRecipient> {
        private readonly ITransport _transport;
        private readonly ActorAddress _recipient;

        public AddressedExpressionTransport(ITransport transport, ActorAddress recipient) {
            _transport = transport;
            _recipient = recipient;
        }

        public void Send(Expression<Action<TRecipient>> message) {
            _transport.Send(new ExpressionActionMessage<TRecipient>(_transport.Sender, _recipient, message));
        }

        public void Send(Expression<Action<TRecipient, MessageMeta>> message) {
            _transport.Send(new ExpressionActionMessageWithMeta<TRecipient>(_transport.Sender, _recipient, message));
        }

        public Result SendAndReceive(Expression<Action<TRecipient>> message) {
            var r = new Result();
            _transport.Send(new ExpressionActionMessage<TRecipient>(_transport.Sender, _recipient, message, r));
            return r;
        }

        public Result SendAndReceive(Expression<Action<TRecipient, MessageMeta>> message) {
            var r = new Result();
            _transport.Send(new ExpressionActionMessageWithMeta<TRecipient>(_transport.Sender, _recipient, message, r));
            return r;
        }

        public Result<TResponse> SendAndReceive<TResponse>(Expression<Func<TRecipient, TResponse>> message) {
            var r = new Result<TResponse>();
            _transport.Send(new ExpressionFuncMessage<TRecipient,TResponse>(_transport.Sender, _recipient, message, r));
            return r;
        }

        public Result<TResponse> SendAndReceive<TResponse>(Expression<Func<TRecipient, MessageMeta, TResponse>> message) {
            var r = new Result<TResponse>();
            _transport.Send(new ExpressionFuncMessageWithMeta<TRecipient, TResponse>(_transport.Sender, _recipient, message, r));
            return r;
        }
    }
}
