using System.Collections;

namespace SpecialTask.Infrastructure.Collections
{
    internal class PairsEnumerator<K, V> : IEnumerator<KeyValuePair<K, V>>
    {
        private readonly Pairs<K, V> pairs;
        private int pointer = -1;

        public PairsEnumerator(Pairs<K, V> map)
        {
            this.pairs = map;
        }

        public KeyValuePair<K, V> Current
        {
            get
            {
                try { return pairs[pointer]; }
                catch (IndexOutOfRangeException) { throw new InvalidOperationException(); }
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            pointer++;
            return pointer < pairs.Count;
        }

        public void Reset()
        {
            pointer = -1;
        }
    }
}
