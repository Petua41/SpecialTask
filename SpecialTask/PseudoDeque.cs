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
            return list.GetEnumerator();
        }

        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
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
                    if (!this[i].Equals(other[i])) return false;
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
}
