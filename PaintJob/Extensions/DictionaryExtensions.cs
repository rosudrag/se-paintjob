using System.Collections.Generic;

namespace PaintJob.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddRange<K, V>(this Dictionary<K, V> dict, Dictionary<K, V> other)
        {
            foreach (KeyValuePair<K, V> keyValuePair in other)
                dict.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }
}