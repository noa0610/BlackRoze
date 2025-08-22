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
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }
        public static HashSet<T> GetEnumHashSet<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToHashSet();
        }
    }
}