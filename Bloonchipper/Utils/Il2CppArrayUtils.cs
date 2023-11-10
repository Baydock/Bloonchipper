using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;

namespace Bloonchipper.Utils {
    internal static class Il2CppArrayUtils {
        /// <summary>
        /// Appends the given values to the end of the Il2CppArray and returns the result
        /// </summary>
        /// <typeparam name="T">The type that is contained in the array</typeparam>
        /// <param name="array">The array to be appended to</param>
        /// <param name="values">The values to be appended</param>
        /// <returns>A new array with the values appended</returns>
        public static T[] Append<T>(this Il2CppArrayBase<T> array, params T[] values) {
            T[] result = new T[array.Length + values.Length];
            array.CopyTo(result, 0);
            values.CopyTo(result, array.Length);
            return result;
        }

        /// <summary>
        /// Adds all of the given values to the end of the List
        /// </summary>
        /// <typeparam name="T">The type that is contained in the List</typeparam>
        /// <param name="list">The list to be added to</param>
        /// <param name="values">The values to be added></param>
        public static void Add<T>(this List<T> list, params T[] values) {
            foreach(T v in values)
                list.Add(v);
        }

        /// <summary>
        /// Inserts the given values at the given index of the given Il2CppArray
        /// </summary>
        /// <typeparam name="T">The type that is contained in the array</typeparam>
        /// <param name="array">The array to be inserted to</param>
        /// <param name="index">The index to be inserted at</param>
        /// <param name="values">The values to be inserted</param>
        /// <returns>A new array with the values appended</returns>
        public static T[] Insert<T>(this Il2CppArrayBase<T> array, int index, params T[] values) {
            T[] added = new T[array.Length + values.Length];
            for (int i = 0; i < index; i++)
                added[i] = array[i];
            values.CopyTo(added, index);
            for (int i = index; i < array.Length; i++)
                added[i + values.Length] = array[i];
            return added;
        }

        /// <summary>
        /// Adds to the given dictionary only if the given key is not already contained
        /// </summary>
        /// <typeparam name="K">The key type</typeparam>
        /// <typeparam name="V">The value type</typeparam>
        /// <param name="dict">The dictionary to be added to</param>
        /// <param name="key">The key to be added</param>
        /// <param name="value">The value to be paired with the key</param>
        public static void AddIfNotPresent<K, V>(this Dictionary<K, V> dict, K key, V value) {
            if (!dict.ContainsKey(key))
                dict.Add(key, value);
        }
    }
}
