using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using heos_remote_lib;

namespace heos_remote_systray
{
    /// <summary>
    /// Machine-readable definition of a grpup.
    /// </summary>
    public class HeosGroupConfig
    {
        public string Name = "";
        public List<List<int>> Index = new();

        public HeosGroupConfig() { }
        public HeosGroupConfig(string config, int numDevices = 999) { Parse(config, numDevices); }

        public bool IsValid() => Name?.HasContent() == true && Index.Count > 0;

        public bool Parse(string str, int numDevices = 999)
        {
            // reset
            Name = "";
            Index = new();

            // access
            if (str?.HasContent() != true)
                return false;

            // outer split
            var outer = str.Split('|');
            if (outer.Length < 1)
                return false;

            Name = outer[0];

            // over outer groups
            foreach (var gstr in outer.Skip(1))
            {
                // inner of group shall be separated by ','
                if (gstr?.HasContent() != true)
                    continue;
                var inner = gstr.Split(",");
                if (inner.Length < 1)
                    continue;
                // collect indices AND CONSTRAIN the INDEX
                var g = new List<int>();
                foreach (var istr in inner)
                {
                    if (!int.TryParse(istr, out int i))
                        continue;
                    if (i < 0 || i >= numDevices)
                        continue;
                    g.Add(i);
                }
                // add group
                if (g.Count < 1)
                    continue;
                Index.Add(g);
            }

            // nice
            return true;
        }

        public async Task<bool> Execute(HeosConnectedItem device)
        {
            // access
            if (device?.Telnet == null)
                return false;

            // find all players
            var o1 = await device.Telnet.SendCommandAsync("heos://player/get_players\r\n");
            if (!HeosTelnet.IsSuccessCode(o1))
                return false;

            // all groups
            foreach (var indexList in Index)
            {
                // access
                if (indexList == null) 
                    continue;

                // collect the pids
                var pids = new List<int>();
                foreach (var ndx1 in indexList)
                {
                    // was 1-based
                    var ndx = ndx1 - 1;
                    if (ndx < 0 || ndx >= o1.payload.Count)
                        continue;
                    // add
                    int i2 = o1.payload[ndx].pid;
                    pids.Add(i2);
                }
                // finally it?
                if (pids.Count < 1)
                    continue;
                // do it
                var pidstr = string.Join(',', pids);
                var o2 = await device.Telnet.SendCommandAsync($"heos://group/set_group?pid={pidstr}\r\n");
                if (!HeosTelnet.IsSuccessCode(o2))
                    return false;
            }

            // nice
            return true;
        }
    }
}
