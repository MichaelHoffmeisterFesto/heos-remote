using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace heos_remote_ctl
{
    public class Options
    {
        [Option('d', "device", Required = true, HelpText = "Friedly name of HEOS device.")]
        public string? Device { get; set; }

        [Option('m', "manufacturer", Required = false, HelpText = "Manufacturer name of HEOS device; first device will be taken.")]
        public string? Manufacturer { get; set; }

        [Option('i', "ifc-name", Required = false, HelpText = "(Network) interface name to take for discovery.")]
        public string? IfcName { get; set; }

        [Option('v', "verbosity", Required = false, HelpText = "Verbosity level >= 0.")]
        public int Verbose { get; set; } = 0;

        [Option('t', "time-out", Required = false, HelpText = "Time-out in [ms].")]
        public int TimeOut { get; set; } = 3000;

        // Singleton
        public static Options Curr = new Options();
    }
}
