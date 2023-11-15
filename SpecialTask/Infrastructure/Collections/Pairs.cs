using System.Data;

namespace SpecialTask.Infrastructure.Collections
{
    public class Pairs<K, V> : List<KeyValuePair<K, V>>
    {
        public Pairs() : base() { }

        public Pairs(IEnumerable<KeyValuePair<K, V>> old) : base(old) { }

        public void Add(K key, V value)
        {
            Add(new(key, value));
        }

        public List<K> Keys => this.Select(x => x.Key).ToList();

        public List<V> Values => this.Select(kvp => kvp.Value).ToList();

        public override bool Equals(object? obj)
        {
            return obj is Pairs<K, V> other && Keys == other.Keys && Values == other.Values;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Pairs<K, V> a, object? b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Pairs<K, V> a, object? b)
        {
            return !(a == b);
        }

        public object Clone()
        {
            return new Pairs<K, V>(this);
        }

        /// <summary>
        /// Creates new Pairs, containing all elements from <paramref name="a"/> and all elements from <paramref name="b"/> 
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
