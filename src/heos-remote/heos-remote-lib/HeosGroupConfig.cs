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

        public async Task<bool> Execute(
            List<HeosDeviceConfig> deviceConfig, 
            HeosConnectedItem activeDevice,
            bool unGroup = false)
        {
            // access
            if (deviceConfig == null || deviceConfig.Count < 1 || activeDevice?.Telnet == null)
                return false;

            // find all players
            var o1 = await activeDevice.Telnet.SendCommandAsync("heos://player/get_players\r\n");
            if (!HeosTelnet.IsSuccessCode(o1))
                return false;

            // un-bind all groups?
            if (unGroup)
            {
                // get all groups
                var o2 = await activeDevice.Telnet.SendCommandAsync("heos://group/get_groups\r\n");
                if (!HeosTelnet.IsSuccessCode(o1))
                    return false;

                // go into that groups
                foreach (var plgrp in o2.payload)
                {
                    int? leadPid = null;
                    foreach (var player in plgrp.players)
                    {
                        if (player == null)
                            continue;
                        string? roleName = player.role?.ToString();
                        if (roleName?.Equals("leader", StringComparison.InvariantCultureIgnoreCase) == true)
                        {
                            int ii = player.pid;
                            leadPid = ii;
                        }
                    }

                    // ungroup leader in order to "free" all others
                    if (leadPid.HasValue)
                    {
                        var o4 = await activeDevice.Telnet.SendCommandAsync($"heos://group/set_group?pid={leadPid.Value}\r\n");
                        if (!HeosTelnet.IsSuccessCode(o4))
                            return false;
                    }
                }
            }

            // build new groups
            foreach (var indexList in Index)
            {
                // access
                if (indexList == null) 
                    continue;

                // collect the pids
                var pids = new List<int>();
                foreach (var ndx1 in indexList)
                {
                    // index is 1-based and indexes the devices as seen by the user!
                    // need the friendly name
                    var ndx = ndx1 - 1;
                    if (ndx < 0 || ndx >= deviceConfig.Count)
                        continue;
                    var ffn = deviceConfig[ndx]?.FriendlyName?.Trim();
                    if (ffn?.HasContent() != true)
                        continue;

                    // find this friendly name in the players list
                    for (int pi=0; pi < o1.payload.Count; pi++)
                    {
                        var pln = o1.payload[pi]?.name?.ToString().Trim();
                        if (ffn.Equals(pln, StringComparison.InvariantCultureIgnoreCase))
                        {
                            int ii = o1.payload[pi].pid;
                            pids.Add(ii);
                            break;
                        }
                    }
                }
                // finally it?
                if (pids.Count < 1)
                    continue;
                // do it
                var pidstr = string.Join(',', pids);
                var o4 = await activeDevice.Telnet.SendCommandAsync($"heos://group/set_group?pid={pidstr}\r\n");
                if (!HeosTelnet.IsSuccessCode(o4))
                    return false;
            }

            // nice
            return true;
        }
    }
}
