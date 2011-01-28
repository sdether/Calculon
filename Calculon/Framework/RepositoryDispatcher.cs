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
    public class RepositoryDispatcher<TRecipient> : IDispatcherLink {
        private readonly IMailboxRepository<TRecipient> _repository;

        public RepositoryDispatcher(IMailboxRepository<TRecipient> repository) {
            _repository = repository;
        }

        public bool Dispatch<TData>(Message<TData> message) {
            return false;
        }

        public bool Dispatch<TRecipientX>(ExpressionMessage<TRecipientX> message) {
            var castMessage = message as ExpressionMessage<TRecipient>;
            if(castMessage == null) {
                return false;
            }
            var mbox = _repository.GetMailbox(message.Meta);
            return mbox.Accept(castMessage);
        }
    }
}