using System;
using System.Threading;
using System.Threading.Tasks;

namespace Droog.Calculon.Tests.Actors {
    public class ConcurrentResult {
        public int Start;
        public int During;
        public int End;
    }

    public interface IConcurrently {
        Task<ConcurrentResult> OnlyOneShallEnter(TimeSpan block);
        Task<ConcurrentResult> OnlyOneShallEnter2(TimeSpan block);
    }

    public class Concurrently : AActor, IConcurrently {


        private int _inOnlyOneShallEnter;

        public Task<ConcurrentResult> OnlyOneShallEnter(TimeSpan block) {
            var r = new ConcurrentResult();
            r.Start = _inOnlyOneShallEnter;
            Thread.Sleep(block);
            r.During = Interlocked.Increment(ref _inOnlyOneShallEnter);
            Thread.Sleep(block);
            r.End = Interlocked.Decrement(ref _inOnlyOneShallEnter);
            return Return(r);
        }
        public Task<ConcurrentResult> OnlyOneShallEnter2(TimeSpan block) {
            var r = new ConcurrentResult();
            r.Start = _inOnlyOneShallEnter;
            Thread.Sleep(block);
            r.During = Interlocked.Increment(ref _inOnlyOneShallEnter);
            Thread.Sleep(block);
            r.End = Interlocked.Decrement(ref _inOnlyOneShallEnter);
            return Return(r);
        }
    }
}