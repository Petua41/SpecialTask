using System.Collections;
using System.Data;

namespace SpecialTask.Infrastructure.Collections
{
    /// <summary>
    /// Ordered collection of key-value pairs, that doesn`t requires uniqueness of keys (not map, but collection of pairs)
    /// </summary>
    public class Pairs<K, V> : List<KeyValuePair<K, V>>, ICloneable, IList
    {
        private readonly List<KeyValuePair<K, V>> map;

        public Pairs()
        {
            map = new();
        }

        public Pairs(IEnumerable<KeyValuePair<K, V>> oldMap) : this()
        {
            foreach (KeyValuePair<K, V> pair in oldMap)
            {
                Add(pair.Key, pair.Value);
            }
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
            return new PairsEnumerator<K, V>(this);
        }

        public List<K> Keys => map.Select(kvp => kvp.Key).ToList();

        public List<V> Values => map.Select(kvp => kvp.Value).ToList();

        public override bool Equals(object? obj)
        {
            return obj is Pairs<K, V> otherMyMap && Keys == otherMyMap.Keys && Values == otherMyMap.Values;
        }

        public override int GetHashCode()
        {
            return map.GetHashCode();
        }

        public object Clone()
        {
            Pairs<K, V> newMap = new();
            foreach (KeyValuePair<K, V> pair in this)
            {
                newMap.Add(pair.Key, pair.Value);
            }

            return newMap;
        }

        public static bool operator ==(Pairs<K, V> a, object? b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Pairs<K, V> a, object? b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Creates new MyMap, consisting of all elements from <paramref name="a"/> and all elements from <paramref name="b"/> 
        /// </summary>
        public static Pairs<K, V> operator +(Pairs<K, V> a, Pairs<K, V> b)
        {
            Pairs<K, V> result = (Pairs<K, V>)a.Clone();
            foreach (KeyValuePair<K, V> kvp in b)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            return result;
        }
    }
}
