using heos_remote_systray;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace heos_remote_lib
{
    /// <summary>
    /// This class holds the decomposed keyboard mapping information of a single mapping.
    /// </summary>
    public class HeosKeyMap
    {
        public string Function = "";
        public ModifierKeys Modifiers;
        public Keys Key;
    }

    public class HeosKeyMapList : List<HeosKeyMap> 
    {
        public HeosKeyMap? FindKey(ModifierKeys modifiers, Keys keys)
        {
            foreach (var key in this)
                if (key.Modifiers == modifiers && key.Key == keys)
                    return key;
            return null;
        }

        public static HeosKeyMapList ParseKeyMappings(IEnumerable<string> kmstrings)
        {
            var res = new HeosKeyMapList();
            foreach (var kmstring in kmstrings)
            {
                // access
                if (kmstring?.HasContent() != true)
                    continue;
                var parts = kmstring.Split('|');
                if (parts.Length != 2)
                    continue;

                // parse inner
                var kd = KeyboardHook.ParseKeyDesignation(parts[1]);
                if (kd == null)
                    continue;

                // add
                res.Add(new HeosKeyMap() { Function = parts[0], Key = kd.Item2, Modifiers = kd.Item1 });
            }
            return res;
        }
    }
}
