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

namespace Droog.Calculon.Framework {
    public class DispatchChain : IDispatcher {

        private readonly IList<IDispatcherLink> _dispatchers = new List<IDispatcherLink>();

        public void Dispatch(IMessage message) {
            (from d in _dispatchers where d.Dispatch(message) select d).FirstOrDefault();
        }

        public void Dispatch<TData>(Message<TData> message) {
            (from d in _dispatchers where d.Dispatch(message) select d).FirstOrDefault();
        }

        public void Dispatch<TRecipient>(ExpressionMessage<TRecipient> message) {
            (from d in _dispatchers where d.Dispatch(message) select d).FirstOrDefault();
        }
    }
}
