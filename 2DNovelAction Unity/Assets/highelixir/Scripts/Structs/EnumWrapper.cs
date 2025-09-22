using System;
using System.Collections.Generic;
using System.Linq;

namespace HighElixir
{
    public static class EnumWrapper
    {
        // おそらく重いので乱用は控える
        /// <typeparam name="T">enum</typeparam>
        /// <returns>特定の列挙型の全ての値を格納したリスト</returns>
        public static List<T> GetEnumList<T>() where T : Enum
        {
            return GetEnumerable<T>().ToList();
        }
        public static HashSet<T> GetEnumHashSet<T>() where T : Enum
        {
            return GetEnumerable<T>().ToHashSet();
        }

        public static string[] GetEnumNames<T>() where T : Enum
        {
            return Enum.GetNames(typeof(T));
        }
        public static Dictionary<T, string> GetDict<T>() where T : Enum
        {
            var values = GetEnumerable<T>();
            var names = GetEnumNames<T>();
            var dict = new Dictionary<T, string>();
            int i = 0;
            foreach (var v in values)
            {
                dict[v] = names[i];
                i++;
            }
            return dict;
        }

        private static IEnumerable<T> GetEnumerable<T>()
            where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}