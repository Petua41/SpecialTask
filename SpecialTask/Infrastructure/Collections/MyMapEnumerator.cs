﻿using System.Collections;

namespace SpecialTask.Infrastructure.Collections
{
    internal class MyMapEnumerator<K, V> : IEnumerator<KeyValuePair<K, V>>
    {
        private readonly Pairs<K, V> map;
        private int pointer = -1;

        public MyMapEnumerator(Pairs<K, V> map)
        {
            this.map = map;
        }

        public KeyValuePair<K, V> Current
        {
            get
            {
                try { return map[pointer]; }
                catch (IndexOutOfRangeException) { throw new InvalidOperationException(); }
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            pointer++;
            return pointer < map.Count;
        }

        public void Reset()
        {
            pointer = -1;
        }
    }
}
