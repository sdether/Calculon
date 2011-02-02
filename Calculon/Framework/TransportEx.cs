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
    public static class TransportEx {

        public static void Send<TRecipient>(this ITransport transport, Expression<Action<TRecipient>> message) {
            transport.Send(new ExpressionActionMessage<TRecipient>(transport.Sender, ActorAddress.Create<TRecipient>(), message));
        }

        public static void Send<TRecipient>(this ITransport transport, Expression<Action<TRecipient, MessageMeta>> message) {
            transport.Send(new ExpressionActionMessageWithMeta<TRecipient>(transport.Sender, ActorAddress.Create<TRecipient>(), message));
        }

        public static Result SendAndReceive<TRecipient>(this ITransport transport, Expression<Action<TRecipient>> message) {
            var r = new Result();
            transport.Send(new ExpressionActionMessage<TRecipient>(transport.Sender, ActorAddress.Create<TRecipient>(), message, r));
            return r;
        }

        public static Result SendAndReceive<TRecipient>(this ITransport transport, Expression<Action<TRecipient, MessageMeta>> message) {
            var r = new Result();
            transport.Send(new ExpressionActionMessageWithMeta<TRecipient>(transport.Sender, ActorAddress.Create<TRecipient>(), message, r));
            return r;
        }

        public static Result<TResponse> SendAndReceive<TRecipient, TResponse>(this ITransport transport, Expression<Func<TRecipient, TResponse>> message) {
            var r = new Result<TResponse>();
            transport.Send(new ExpressionFuncMessage<TRecipient, TResponse>(transport.Sender, ActorAddress.Create<TRecipient>(), message, r));
            return r;
        }

        public static Result<TResponse> SendAndReceive<TRecipient, TResponse>(this ITransport transport, Expression<Func<TRecipient, MessageMeta, TResponse>> message) {
            var r = new Result<TResponse>();
            transport.Send(new ExpressionFuncMessageWithMeta<TRecipient, TResponse>(transport.Sender, ActorAddress.Create<TRecipient>(), message, r));
            return r;
        }

        public static IAddressedExpressionTransport<TRecipient> For<TRecipient>(this ITransport transport, string id) {
            return new AddressedExpressionTransport<TRecipient>(transport, ActorAddress.Create<TRecipient>(id));
        }

        public static void Send<TMessageData>(this ITransport transport, TMessageData messageData) {
            transport.Send(new Message<TMessageData>(messageData, transport.Sender, ActorAddress.Unknown));
        }

        public static Result<TOut> SendAndReceive<TOut, TIn>(this ITransport transport, TIn messageData) {
            throw new NotImplementedException();
        }

        public static IAddressedMessageTransport For(this ITransport transport, string id) {
            throw new NotImplementedException();
        }
    }
}
