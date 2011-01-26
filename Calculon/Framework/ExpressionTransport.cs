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
    public class ExpressionTransport : IExpressionTransport {

        private readonly IGrip _grip;
        private readonly MessageMeta _sender;

        public ExpressionTransport(string senderId, Type senderType, IGrip grip) {
            _grip = grip;
            _sender = new MessageMeta(senderId, senderType);
        }

        public void Send<TRecipient>(Expression<Action<TRecipient>> message) {
            _grip.Route(new ExpressionActionMessage<TRecipient>(_sender.For<TRecipient>(), message));
        }

        public void Send<TRecipient>(Expression<Action<TRecipient, MessageMeta>> message) {
            _grip.Route(new ExpressionActionMessageWithMeta<TRecipient>(_sender.For<TRecipient>(), message));
        }

        public Result SendAndReceive<TRecipient>(Expression<Action<TRecipient>> message) {
            var r = new Result();
            _grip.Route(new ExpressionActionMessage<TRecipient>(_sender.For<TRecipient>(), message, r));
            return r;
        }

        public Result SendAndReceive<TRecipient>(Expression<Action<TRecipient, MessageMeta>> message) {
            var r = new Result();
            _grip.Route(new ExpressionActionMessageWithMeta<TRecipient>(_sender.For<TRecipient>(), message, r));
            return r;
        }

        public Result<TResponse> SendAndReceive<TRecipient, TResponse>(Expression<Func<TRecipient, TResponse>> message) {
            var r = new Result<TResponse>();
            _grip.Route(new ExpressionFuncMessage<TRecipient,TResponse>(_sender.For<TRecipient>(), message, r));
            return r;
        }

        public Result<TResponse> SendAndReceive<TRecipient, TResponse>(Expression<Func<TRecipient, MessageMeta, TResponse>> message) {
            var r = new Result<TResponse>();
            _grip.Route(new ExpressionFuncMessageWithMeta<TRecipient, TResponse>(_sender.For<TRecipient>(), message, r));
            return r;
        }

        public IAddressedExpressionTransport<TRecipient> For<TRecipient>(string id) {
            return new AddressedExpressionTransport<TRecipient>(_grip, _sender.For<TRecipient>(id));
        }
    }

    public class AddressedExpressionTransport<TRecipient> : IAddressedExpressionTransport<TRecipient> {
        private readonly IGrip _grip;
        private readonly MessageMeta _meta;

        public AddressedExpressionTransport(IGrip grip, MessageMeta meta) {
            _grip = grip;
            _meta = meta;
        }

        public void Send(Expression<Action<TRecipient>> message) {
            _grip.Route(new ExpressionActionMessage<TRecipient>(_meta, message));
        }

        public void Send(Expression<Action<TRecipient, MessageMeta>> message) {
            _grip.Route(new ExpressionActionMessageWithMeta<TRecipient>(_meta, message));
        }

        public Result SendAndReceive<TResponse>(Expression<Action<TRecipient>> message) {
            var r = new Result();
            _grip.Route(new ExpressionActionMessage<TRecipient>(_meta, message, r));
            return r;
        }

        public Result SendAndReceive<TResponse>(Expression<Action<TRecipient, MessageMeta>> message) {
            var r = new Result();
            _grip.Route(new ExpressionActionMessageWithMeta<TRecipient>(_meta, message, r));
            return r;
        }

        public Result<TResponse> SendAndReceive<TResponse>(Expression<Func<TRecipient, TResponse>> message) {
            var r = new Result<TResponse>();
            _grip.Route(new ExpressionFuncMessage<TRecipient, TResponse>(_meta, message, r));
            return r;
        }

        public Result<TResponse> SendAndReceive<TResponse>(Expression<Func<TRecipient, MessageMeta, TResponse>> message) {
            var r = new Result<TResponse>();
            _grip.Route(new ExpressionFuncMessageWithMeta<TRecipient, TResponse>(_meta, message, r));
            return r;
        }
    }
}
