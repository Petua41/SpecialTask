using System.Collections;

namespace SpecialTask.Infrastructure
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
