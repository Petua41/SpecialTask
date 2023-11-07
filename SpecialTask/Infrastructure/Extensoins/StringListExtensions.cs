namespace SpecialTask.Infrastructure.Extensoins
{
    /// <summary>
    /// Provides some extensions to <see cref="IList{T}"/> of <see cref="string"/>s
    /// </summary>
    internal static class StringListExtensions
    {
        /// <summary>
        /// Length of the shortest string in the <paramref name="collection"/>
        /// </summary>
        public static int ShortestLength(this IList<string> collection)
        {
            return collection.Select(s => s.Length).Min();
        }

        /// <summary>
        /// The longest common prefix of all strings in <paramref name="collection"/> or <see cref="string.Empty"/> if <paramref name="collection"/> is empty
        /// </summary>
        public static string LongestCommonPrefix(this IList<string> collection)
        {
            if (collection.Count == 0)
            {
                return string.Empty;
            }

            string lastPrefix = string.Empty;
            for (int i = 0; i < collection.ShortestLength(); i++)
            {
                string commonPrefix = collection[0][..(i + 1)];
                foreach (string str in collection)
                {
                    if (!str.StartsWith(commonPrefix))
                    {
                        return lastPrefix;
                    }
                }
                lastPrefix = commonPrefix;
            }
            return lastPrefix;
        }

        /// <summary>
        /// Removes prefix from all <see cref="string"/>s in <paramref name="collection"/>
        /// </summary>
        /// 
        public static IList<string> RemovePrefix(this IList<string> collection, string prefix)
        {
            return collection.Select(st => st.StartsWith(prefix) ? st[prefix.Length..] : st).ToList();
        }
    }
}
