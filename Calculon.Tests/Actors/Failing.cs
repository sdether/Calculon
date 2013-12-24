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
using System.Threading;
using System.Threading.Tasks;

namespace Droog.Calculon.Tests.Actors {

    public class Failure : Exception {}

    public interface IFailing {
        Task FailImmediate();
        Task FailLater();
        Task FailByThrowing();
        Task<int> FailImmediateOfT();
        Task<int> FailLaterOfT();
        Task<int> FailByThrowingOfT();
        void VoidFailure();
        Task<int> VoidFailureCalls();
    }

    public class Failing : AActor, IFailing {
        private int _voidFailureCalled;

        public Task FailImmediate() {
            var completion = Context.GetCompletion();
            completion.Fail(new Failure());
            return completion;
        }

        public Task FailLater() {
            var completion = Context.GetCompletion();
            new Timer(_ => completion.Fail(new Failure()), null, 100, Timeout.Infinite);
            return completion;
        }

        public Task FailByThrowing() {
            throw new Failure();
        }

        public Task<int> FailImmediateOfT() {
            var completion = Context.GetCompletion<int>();
            completion.Fail(new Failure());
            return completion;
        }

        public Task<int> FailLaterOfT() {
            var completion = Context.GetCompletion<int>();
            new Timer(_ => completion.Fail(new Failure()), null, 100, Timeout.Infinite);
            return completion;
        }

        public Task<int> FailByThrowingOfT() {
            throw new Failure();
        }

        public void VoidFailure() {
            _voidFailureCalled++;
            throw new Failure();
        }

        public Task<int> VoidFailureCalls() {
            return Return(_voidFailureCalled);
        }
    }
}