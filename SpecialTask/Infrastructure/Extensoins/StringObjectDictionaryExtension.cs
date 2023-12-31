﻿namespace SpecialTask.Infrastructure.Extensoins
{
    public static class StringObjectDictionaryExtension
    {
        /// <summary>
        /// Creates an array of values, that can be passed as params.
        /// Alternative to ** in Python.
        /// </summary>
        /// <param name="dict">Dictionary, containing parameter names and values</param>
        /// <param name="keysOrder">ORDERED array of parameter names</param>
        /// <exception cref="KeyNotFoundException"></exception>
        public static object[] Unpack<T>(this Dictionary<string, T> dict, string[] keysOrder)
        {
            object[] result = new object[keysOrder.Length];

            for (int i = 0; i < keysOrder.Length; i++)
            {
                string key = keysOrder[i];

                if (dict.TryGetValue(key, out T? value))
                {
                    result[i] = value!;     // value cannot be null, if TryGetValue is true. So we forgive null
                }
                else
                {
                    throw new KeyNotFoundException($"Key {key} not found in dictionary while unpacking it");
                }
            }

            return result;
        }

        /// <summary>
        /// Creates an array of values, that can be passed as params.
        /// Alternative to ** in Python.
        /// </summary>
        /// <param name="dict">Dictionary, containing parameter names and values</param>
        /// <param name="keysOrder">Space-separated parameter names</param>
        /// <exception cref="KeyNotFoundException"></exception>
        public static object[] Unpack<T>(this Dictionary<string, T> dict, string keysOrder)
        {
            return dict.Unpack(keysOrder.SplitInsensitive(' '));
        }
    }
}
