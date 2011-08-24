using System.Collections.Generic;

namespace ClientDependency.Core
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Determines if 2 dictionaries contain the exact same keys/values
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="d"></param>
        /// <param name="compareTo"></param>
        /// <returns></returns>
        public static bool IsEqualTo<TKey, TVal>(this IDictionary<TKey, TVal> d, IDictionary<TKey, TVal> compareTo)
        {
            if (d.Count != compareTo.Count)
                return false;

            foreach(var i in d)
            {
                if (!compareTo.ContainsKey(i.Key))
                    return false;
                if (compareTo[i.Key].Equals(i.Value))
                    return false;
            }

            return true;
        }

    }
}