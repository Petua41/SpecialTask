using System.Collections.Generic;
using System.Linq;

namespace SpecialTask
{
    /// <summary>
    /// Представляет упорядоченную коллекцию пар "ключ-значение", не требующую уникальность ключей
    /// </summary>
    public class MyMap<K, V> : List<KeyValuePair<K, V>>
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

        public new int Capacity => map.Capacity;

        public new KeyValuePair<K, V> this[int index]
        {
            get => map[index];
            set => map[index] = value;
        }

        public new IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return map.GetEnumerator();
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

        public static bool operator ==(MyMap<K, V> a, object? b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(MyMap<K, V> a, object? b)
        {
            return !(a == b);
        }
    }
}
