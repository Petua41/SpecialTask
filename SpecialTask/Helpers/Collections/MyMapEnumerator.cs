﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace SpecialTask.Helpers
{
    class MyMapEnumerator<K, V> : IEnumerator<KeyValuePair<K, V>>
    {
        private readonly MyMap<K, V> map;
        private int pointer = -1;

        public MyMapEnumerator(MyMap<K, V> map)
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