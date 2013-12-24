/* ----------------------------------------------------------------------------
 * Copyright (C) 2013 Arne F. Claassen
 * geekblog [at] claassen [dot] net
 * http://github.com/sdether/Calculon
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 * ----------------------------------------------------------------------------
 */

using System;
using System.Threading.Tasks;

namespace Droog.Calculon.Backstage {
    public class ActorContext : IActorContext {
        private readonly IBackstage _backstage;
        private readonly ActorRef _self;
        private readonly ActorRef _parent;

        public ActorContext(IBackstage backstage, ActorRef self, ActorRef parent) {
            _backstage = backstage;
            _self = self;
            _parent = parent;
        }

        public ActorRef Self { get { return _self; } }
        public ActorRef Parent { get { return _parent; } }

        public Task<TResult> Return<TResult>(TResult value) {
            return TaskHelpers.GetCompletedTask(value);
        }

        public Task Return() {
            return TaskHelpers.CompletedTask;
        }

        public Completion<TResult> GetCompletion<TResult>() {
            return new Completion<TResult>();
        }

        public Completion GetCompletion() {
            return new Completion();
        }

        public ActorProxy<TActor> Create<TActor>(string name = null, Func<TActor> builder = null) where TActor : class {
            return _backstage.Create(Self, Self, name: name, builder: builder);
        }

        public ActorProxy<TActor> Find<TActor>(ActorRef actorRef) where TActor : class {
            return _backstage.Find<TActor>(Self, actorRef);
        }
    }
}