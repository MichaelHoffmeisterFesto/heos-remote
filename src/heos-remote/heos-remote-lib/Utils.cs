using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace heos_remote_lib
{
    public static class Utils
    {
        public static bool HasContent(this string str)
        {
            return str != null && str.Trim() != "";
        }

        public static IEnumerable<T> ForEachSafe<T>(this List<T> list)
        {
            if (list == null)
                yield break;
            foreach (var x in list)
                yield return x;
        }

        public static IEnumerable<T> ForEachSafe<T>(this IEnumerable<T> list)
        {
            if (list == null)
                yield break;
            foreach (var x in list)
                yield return x;
        }
    }
}
