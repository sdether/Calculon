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
using Droog.Calculon.Backstage;

namespace Droog.Calculon {

    public enum SupervisionAction {
        Resume,
        Restart,
        Terminate,
        Escalate
    }

    public interface IActor {
        IActorContext Context { get; set; }
        ActorRef Sender { get; set; }
        Task<SupervisionAction> OnDescendantFault(FailureMessage failure);
        Task<object> OnUnknownMessage(Message message);
        Task Start();
        Task Restart();
        Task Stop();
    }

    public abstract class AActor: IActor {

        IActorContext IActor.Context { get { return Context; } set { Context = value; } }
        ActorRef IActor.Sender { get { return Sender; } set { Sender = value; } }
        public virtual Task<SupervisionAction> OnDescendantFault(FailureMessage failure) {
            return Return(SupervisionAction.Resume);
        }

        public virtual Task<object> OnUnknownMessage(Message message) {
            var completion = Context.GetCompletion<object>();
            completion.Fault(new UnhandledMessageException());
            return completion;
        }

        public virtual Task Start() {
            return Return();
        }

        public virtual Task Restart() {
            return Return();
        }

        public virtual Task Stop() {
            return Return();
        }

        protected IActorContext Context { get; set; }
        protected ActorRef Sender { get; set; }

        protected Task<TResult> Return<TResult>(TResult value) {
            return Context.Return(value);
        }

        protected Task Return() {
            return Context.Return();
        }
    }

    public class UnhandledMessageException : Exception {}
}