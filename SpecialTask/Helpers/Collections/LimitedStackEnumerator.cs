using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialTask.Helpers
{
    class LimitedStackEnumerator<T> : IEnumerator<T>
    {
        private int pointer = -1;
        private readonly LimitedStack<T> deque;

        public LimitedStackEnumerator(LimitedStack<T> deque)
        {
            this.deque = deque;
        }

        public T Current
        {
            get
            {
                try { return deque[pointer]; }
                catch (IndexOutOfRangeException) { throw new InvalidOperationException(); }
            }
        }

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            pointer++;
            return pointer < deque?.Count;
        }

        public void Reset()
        {
            pointer = -1;
        }

        public void Dispose() { }
    }
}
