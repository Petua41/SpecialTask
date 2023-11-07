namespace SpecialTask.Infrastructure.Collections
{
    /// <summary>
    /// It`s like restricted <see cref="Stack{T}"/>, but number of it`s elements is never greater than capacity
    /// </summary>
    public class LimitedStack<T>
    {
        private readonly List<T> list;
        private readonly int capacity;

        public LimitedStack(int capacity)
        {
            list = new();
            this.capacity = capacity;
        }

        /// <summary>
        /// Add <paramref name="item"/> to the top of <see cref="LimitedStack{T}"/>
        /// If capacity reached, item on the bottom is removed.
        /// </summary>
        public void Push(T item)
        {
            while (list.Count >= capacity)
            {
                PopBottom();
            }

            list.Add(item);
        }

        /// <exception cref="InvalidOperationException">Stack is empty (underflow)</exception>
        public T Pop()
        {
            if (list.Count == 0)
            {
                throw new InvalidOperationException();
            }

            T result = list[^1];
            list.RemoveAt(list.Count - 1);
            return result;
        }

        public T Peek()
        {
            return list[^1];
        }

        private T PopBottom()
        {
            T result = list[0];
            list.RemoveAt(0);
            return result;
        }
    }
}
