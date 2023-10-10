using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;

namespace SpecialTask
{
    /// <summary>
    /// Представляет упорядоченную коллекцию пар "ключ-значение", не требующую уникальность ключей
    /// </summary>
    public class MyMap<K, V> : List<KeyValuePair<K, V>>, ICloneable, IList
    {
        private readonly List<KeyValuePair<K, V>> map;

        public MyMap()
        {
            map = new();
        }

        public MyMap(IEnumerable<KeyValuePair<K, V>> oldMap) : this()
        {
            foreach (KeyValuePair<K, V> pair in oldMap) Add(pair.Key, pair.Value);
        }

        public void Add(K key, V value)
        {
            map.Add(new KeyValuePair<K, V>(key, value));
        }

        public new void RemoveAt(int index)
        {
            map.RemoveAt(index);
        }

        public new int Count => map.Count;

        int ICollection.Count => Count;

        public new int Capacity => map.Capacity;

        public new KeyValuePair<K, V> this[int index]
        {
            get => map[index];
            set => map[index] = value;
        }

        public new IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return new MyMapEnumerator<K, V>(this);
        }

        public List<K> Keys
        {
            get => (from kvp in map select kvp.Key).ToList();
        }

        public List<V> Values
        {
            get => (from kvp in map select kvp.Value).ToList();
        }

        public override bool Equals(object? obj)
        {
            if (obj is MyMap<K, V> otherMyMap)
            {
                return Keys == otherMyMap.Keys && Values == otherMyMap.Values;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return map.GetHashCode();
        }

        public object Clone()
        {
            MyMap<K, V> newMap = new();
            foreach (KeyValuePair<K, V> pair in this) newMap.Add(pair.Key, pair.Value);
            return newMap;
        }

        public static bool operator ==(MyMap<K, V> a, object? b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(MyMap<K, V> a, object? b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Creates new MyMap, consisting of all elements from <paramref name="a"/> and all elements from <paramref name="b"/> 
        /// </summary>
        public static MyMap<K, V> operator +(MyMap<K, V> a, MyMap<K, V> b)
        {
            MyMap<K, V> result = (MyMap<K, V>)a.Clone();
            foreach (KeyValuePair<K, V> kvp in b) result.Add(kvp.Key, kvp.Value);
            return result;
        }
    }

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
