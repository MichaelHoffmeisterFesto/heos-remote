using System;
using System.Collections.Generic;
using System.Dynamic;
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

        public static bool EvalBool(string st)
        {
            if (st?.HasContent() != true)
                return false;
            if ("yes true 1".Contains(st.Trim().ToLower()))
                return true;
            return false;
        }

        // see: https://stackoverflow.com/questions/9956648/how-do-i-check-if-a-property-exists-on-a-dynamic-anonymous-type-in-c
        public static bool PropertyExists(dynamic obj, string name)
        {
            if (obj == null) return false;

            else if (obj is IDictionary<string, object> dict)
            {
                return dict.ContainsKey(name);
            }

            else if (obj is Newtonsoft.Json.Linq.JObject jObject)
            {
                return jObject.ContainsKey(name);
            }

            else
            {
                return obj.GetType().GetProperty(name) != null;
            }
        }
    }
}
