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

using System.Threading.Tasks;

namespace Droog.Calculon.Tests.Actors {

    public interface IEcho {
        Task<string> Ask(string data);
        Task Notify();
        void Tell();
        Task<bool> VerifyTell();
    }

    public class Echo : AActor, IEcho {

        private bool _wasTold;
        
        public Task<string> Ask(string data) {
            return Return(data);
        }

        public Task Notify() {
            return Return();
        }

        public void Tell() {
            _wasTold = true;
        }

        public Task<bool> VerifyTell() {
            return Return(_wasTold);
        }
    }
}