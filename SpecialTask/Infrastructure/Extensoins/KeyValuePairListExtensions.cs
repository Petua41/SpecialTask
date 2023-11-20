namespace SpecialTask.Infrastructure.Extensoins
{
    public static class KeyValuePairListExtensions
    {
        /// <summary>
        /// Shorthand to add <see cref="KeyValuePair"/> to list by adding <paramref name="key"/> and <paramref name="value"/>
        /// </summary>
        public static void Add<K, V>(this List<KeyValuePair<K, V>> list, K key, V value)
        {
            list.Add(new(key, value));
        }

        /// <summary>
        /// Shorthand to get keys from <paramref name="list"/>`s <see cref="KeyValuePair"/>s
        /// </summary>
        public static List<K> Keys<K, V>(this List<KeyValuePair<K, V>> list)
        {
            return list.Select(pair => pair.Key).ToList();
        }

        /// <summary>
        /// Creates new <see cref="List{KeyValuePair}"/>, containing all elements from <paramref name="a"/> and then from <paramref name="b"/>
        /// </summary>
        public static List<KeyValuePair<K, V>> Concatenate<K, V>(this List<KeyValuePair<K, V>> a, List<KeyValuePair<K, V>> b)
        {
            List<KeyValuePair<K, V>> result = new();

            foreach (KeyValuePair<K, V> kvp in a)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<K, V> kvp in b)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            return result;
        }
    }
}
