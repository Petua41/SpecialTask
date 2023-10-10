using System;
using System.Collections;
using System.Collections.Generic;

namespace SpecialTask
{
    /// <summary>
    /// Стэк, к которому добавлен метод PopBottom. Реализован на List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PseudoDeque<T> : IEnumerable<T>, ICollection<T>
    {
        private readonly List<T> list;

        public PseudoDeque()
        {
            list = new();
        }

        public PseudoDeque(IEnumerable<T> collection)
        {
            list = new(collection);
        }

        public int Count => list.Count;

        public bool IsReadOnly => ((ICollection<T>)list).IsReadOnly;

        public void Add(T item)
        {
            Push(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new PseudoDequeEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        public void Push(T item)
        {
            list.Add(item);
        }

        public T Pop()
        {
            if (list.Count == 0) throw new UnderflowException();
            T result = list[^1];
            list.RemoveAt(list.Count - 1);
            return result;
        }

        public T PopBottom()
        {
            T result = list[0];
            list.RemoveAt(0);
            return result;
        }

        public T Peek()
        {
            return list[^1];
        }

        public T this[int index]
        {
            get
            {
                return list[index];
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is PseudoDeque<T> other && Count == other.Count)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (this[i] == null && other[i] == null) return true;
                    if (!this[i]?.Equals(other[i]) ?? false) return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return list.GetHashCode();
        }

        public override string ToString()
        {
            return "PseudoDeque { " + string.Join(' ', list) + " }";
        }
    }

    class PseudoDequeEnumerator<T> : IEnumerator<T>
    {
        private int pointer = -1;
        private readonly PseudoDeque<T> deque;

        public PseudoDequeEnumerator(PseudoDeque<T> deque)
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
