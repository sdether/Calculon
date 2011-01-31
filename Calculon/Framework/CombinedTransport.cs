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
    public class CombinedTransport : ICombinedTransport {
        private readonly ExpressionTransport _expressionTransport;
        private readonly MessageTransport _messageTransport;

        public CombinedTransport(ActorAddress sender, IDispatcher dispatcher) {
            _expressionTransport = new ExpressionTransport(sender, dispatcher);
            _messageTransport = new MessageTransport(sender, dispatcher);
        }

        void IMessageTransport.Send<TMessageData>(TMessageData messageData) {
            _messageTransport.Send(messageData);
        }

        Result<TOut> IMessageTransport.SendAndReceive<TOut, TIn>(TIn messageData) {
            return _messageTransport.SendAndReceive<TOut,TIn>(messageData);
        }

        IAddressedMessageTransport IMessageTransport.For(string id) {
            return _messageTransport.For(id);
        }

        void IExpressionTransport.Send<TRecipient>(Expression<Action<TRecipient>> message) {
            _expressionTransport.Send(message);
        }

        void IExpressionTransport.Send<TRecipient>(Expression<Action<TRecipient, MessageMeta>> message) {
            _expressionTransport.Send(message);
        }

        Result IExpressionTransport.SendAndReceive<TRecipient>(Expression<Action<TRecipient>> message) {
            return _expressionTransport.SendAndReceive(message);
        }

        Result IExpressionTransport.SendAndReceive<TRecipient>(Expression<Action<TRecipient, MessageMeta>> message) {
            return _expressionTransport.SendAndReceive(message);
        }

        Result<TResponse> IExpressionTransport.SendAndReceive<TRecipient, TResponse>(Expression<Func<TRecipient, TResponse>> message) {
            return _expressionTransport.SendAndReceive(message);
        }

        Result<TResponse> IExpressionTransport.SendAndReceive<TRecipient, TResponse>(Expression<Func<TRecipient, MessageMeta, TResponse>> message) {
            return _expressionTransport.SendAndReceive(message);
        }

        IAddressedExpressionTransport<TRecipient> IExpressionTransport.For<TRecipient>(string id) {
            return _expressionTransport.For<TRecipient>(id);
        }
    }
}