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
using Droog.Calculon.Messages;
using MindTouch.Collections;

namespace Droog.Calculon.Framework {
    public class Mailbox<TRecipient> : IMailbox<TRecipient> {

        private readonly ProcessingQueue<ExpressionMessage<TRecipient>> _queue;
        private readonly ActorAddress _address;
        private readonly TRecipient _recipient;
        private bool _isAlive = true;

        public Mailbox(ActorAddress address, TRecipient recipient, int parallelism) {
            _address = address;
            _recipient = recipient;
            _queue = new ProcessingQueue<ExpressionMessage<TRecipient>>(Dispatch, parallelism);
        }

        public bool Accept(ExpressionMessage<TRecipient> message) {
            _queue.TryEnqueue(message);
            return true;
        }

        private void Dispatch(ExpressionMessage<TRecipient> message) {
            message.Invoke(_recipient);
        }

        public ActorAddress Recipient { get { return _address; } }

        public bool Accept(IMessage message) {
            if(message is ShutdownMessage) {
                var disposable = _recipient as IDisposable;
                if(disposable != null) {
                    disposable.Dispose();
                }
                _isAlive = false;
                return true;
            }
            return false;
        }

        public bool Accept<TData>(Message<TData> message) {
            return false;
        }

        public bool IsAlive {get { return _isAlive; }}

        public void Dispose() {}
    }
}