using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;

namespace heos_remote_lib
{
    /// <summary>
    /// Device information prodided by the options
    /// </summary>
    public class HeosDeviceConfig
    {
        public string FriendlyName = "";
        public IPEndPoint? EndPoint = null;
    }

    /// <summary>
    /// Set of options to be changed by command line parsing
    /// </summary>
    public class HeosAppOptions
    {
        [Option('d', "device", Required = false, HelpText = "Sequence of one to many friedly name of HEOS device(s). Each may be of format name|host:port to disable auto discovery.")]
        public IEnumerable<string> Devices { get; set; } = Enumerable.Empty<string>();

        [Option('g', "group", Required = false, HelpText = "Sequence of zero to many group definitions. Each is of format Name|<index>,..,<index>|..|<index>,..,<index>|, with <index> being 1-based device indices.")]
        public IEnumerable<string> Groups { get; set; } = Enumerable.Empty<string>();

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

        [Option('k', "key-map", Required = false, HelpText = "Create system-wide global keyboard shortcut. Format e.g. Play|Control+Shift+F8.")]
        public IEnumerable<string> KeyMap { get; set; } = Enumerable.Empty<string>();

        [Option("cids", HelpText = "Sequence of tuples, which are starting points for containers. Format name|sid|cid." )]
        public IEnumerable<string> StartCids { get; set; } = Enumerable.Empty<string>();

        [Option('j', "read-json", Required = false, HelpText = "Path to configuration options in JSON format.")]
        public string? ReadJsonFile { get; set; }

        /// <summary>
        /// Will read options from a file into the given instance.
        /// </summary>
        public static void ReadJson(string fn, HeosAppOptions optionsInformation)
        {
            try
            {
                var jsonStr = System.IO.File.ReadAllText(fn);
                JsonConvert.PopulateObject(jsonStr, optionsInformation);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("When reading options JSON file: {0}", ex.Message.ToString());
            }
        }

        public static HeosDeviceConfig? SplitDeviceName(string? deviceName)
        {
            // access
            if (deviceName?.HasContent() != true)
                return null;

            // split?
            var parts = deviceName.Split('|');
            if (parts.Length != 2)
                return new HeosDeviceConfig() { FriendlyName = parts[0] };
            
            // 2 parts
            return new HeosDeviceConfig() { FriendlyName = parts[0], EndPoint = IPEndPoint.Parse(parts[1]) };
        }

        public IEnumerable<HeosDeviceConfig> GetDeviceConfigs()
        {
            foreach (var dev in Devices)
            {
                var ds = SplitDeviceName(dev);
                if (ds != null)
                    yield return ds;
            }
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
