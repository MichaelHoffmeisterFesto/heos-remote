﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace heos_remote_lib
{
    public class HeosAppOptions
    {
        [Option('d', "device", Required = true, HelpText = "Sequence of one to many friedly name of HEOS device(s). Each may be of format name|host:port to disable auto discovery.")]
        public IEnumerable<string> Devices { get; set; } = Enumerable.Empty<string>();

        [Option('m', "manufacturer", Required = false, HelpText = "Manufacturer name of HEOS device; first device will be taken.")]
        public string? Manufacturer { get; set; }

        [Option('i', "ifc-name", Required = false, HelpText = "(Network) interface name to take for discovery.")]
        public string? IfcName { get; set; }

        [Option('v', "verbosity", Required = false, HelpText = "Verbosity level >= 0.")]
        public int Verbose { get; set; } = 0;

        [Option('t', "time-out", Required = false, HelpText = "Time-out in [ms].")]
        public int TimeOut { get; set; } = 3000;

        [Option('u', "username", Required = false, HelpText = "Username of the HEOS account (cleartext).")]
        public string? Username { get; set; }

        [Option('p', "password", Required = false, HelpText = "Password of the HEOS account (cleartext, sigh!).")]
        public string? Password { get; set; }

        [Option("cids", HelpText = "Sequence of tuples, which are starting points for containers. Format name|sid|cid." )]
        public IEnumerable<string> StartCids { get; set; } = Enumerable.Empty<string>();

        public static Tuple<string?, IPEndPoint?> SplitDeviceName(string? deviceName)
        {
            // access
            if (deviceName?.HasContent() != true)
                return new Tuple<string?, IPEndPoint?>(null, null);

            // split?
            var parts = deviceName.Split('|');
            if (parts.Length != 2)
                return new Tuple<string?, IPEndPoint?>(parts[0], null);
            
            // 2 parts
            return new Tuple<string?, IPEndPoint?>(parts[0], IPEndPoint.Parse(parts[1]));
        }

        public IEnumerable<HeosContainerLocation> GetStartPoints()
        {
            if (StartCids != null)
                foreach (var tstr in StartCids)
                {
                    var tup = tstr?.Split('|');
                    if (tup == null || tup.Length != 3)
                        continue;
                    if (!int.TryParse(tup[1], out var sid))
                        continue;
                    yield return new HeosContainerLocation()
                    {
                        Name = tup[0].Trim(),
                        Sid = sid,
                        Cid = tup[2].Trim()
                    };
                }
        }
    }
}
