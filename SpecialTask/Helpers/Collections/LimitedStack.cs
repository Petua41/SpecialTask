using System;
using System.Collections;
using System.Collections.Generic;

namespace SpecialTask.Helpers
{
    /// <summary>
    /// It`s like <see cref="Stack{T}"/>, but number of it`s elements is never greater than capacity
    /// </summary>
    public class LimitedStack<T> : IEnumerable<T>, ICollection<T>
    {
        private readonly List<T> list;
        private readonly int capacity;

        public LimitedStack(int capacity)
        {
            list = new();
            this.capacity = capacity;
        }

        public LimitedStack(IEnumerable<T> collection, int capacity)
        {
            list = new(collection);
            this.capacity = capacity;
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
            return new LimitedStackEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        /// <summary>
        /// Add <paramref name="item"/> to the top of <see cref="LimitedStack{T}"/>
        /// If capacity reached, item on the bottom is removed.
        /// </summary>
        public void Push(T item)
        {
            while (list.Count >= capacity) PopBottom();
            list.Add(item);
        }

        /// <exception cref="InvalidOperationException">Stack is empty (underflow)</exception>
        public T Pop()
        {
            if (list.Count == 0) throw new InvalidOperationException();
            T result = list[^1];
            list.RemoveAt(list.Count - 1);
            return result;
        }

        public T Peek()
        {
            return list[^1];
        }

        public T this[int index] => list[index];

        public override bool Equals(object? obj)
        {
            if (obj is LimitedStack<T> other && Count == other.Count)
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

        private T PopBottom()
        {
            T result = list[0];
            list.RemoveAt(0);
            return result;
        }
    }
}
