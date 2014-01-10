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
    public class ConcurrentResult {
        public int Start;
        public int During;
        public int End;
    }

    public interface IConcurrently : IActor {
        Task<ConcurrentResult> OnlyOneShallEnter(TimeSpan block);
        Task<ConcurrentResult> OnlyOneShallEnter2(TimeSpan block);
    }

    public class Concurrently : AActor, IConcurrently {

        private int _inOnlyOneShallEnter;
        private readonly Random _random = new Random();

        public Task<ConcurrentResult> OnlyOneShallEnter(TimeSpan block) {
            var r = new ConcurrentResult();
            r.Start = _inOnlyOneShallEnter;
            Thread.Sleep(_random.Next((int)block.TotalMilliseconds));
            r.During = Interlocked.Increment(ref _inOnlyOneShallEnter);
            Thread.Sleep(_random.Next((int)block.TotalMilliseconds));
            r.End = Interlocked.Decrement(ref _inOnlyOneShallEnter);
            return Return(r);
        }

        public Task<ConcurrentResult> OnlyOneShallEnter2(TimeSpan block) {
            var r = new ConcurrentResult();
            r.Start = _inOnlyOneShallEnter;
            Thread.Sleep(_random.Next((int)block.TotalMilliseconds));
            r.During = Interlocked.Increment(ref _inOnlyOneShallEnter);
            Thread.Sleep(_random.Next((int)block.TotalMilliseconds));
            r.End = Interlocked.Decrement(ref _inOnlyOneShallEnter);
            return Return(r);
        }
    }
}