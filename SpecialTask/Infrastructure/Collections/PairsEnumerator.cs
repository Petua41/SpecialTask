using System.Collections;

namespace SpecialTask.Infrastructure.Collections
{
    internal class PairsEnumerator<K, V> : IEnumerator<KeyValuePair<K, V>>
    {
        private readonly Pairs<K, V> pairs;
        private int pointer = -1;

        public PairsEnumerator(Pairs<K, V> pairs)
        {
            this.pairs = pairs;
        }

        public KeyValuePair<K, V> Current
        {
            get
            {
                try { return pairs[pointer]; }
                catch (IndexOutOfRangeException e) { throw new InvalidOperationException("Enumerate operation cannot happen: index out of range", e); }
                // List throws InvalidOperationException with message "EnumOpCantHappen" in this situation, so we`ll do the same
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
