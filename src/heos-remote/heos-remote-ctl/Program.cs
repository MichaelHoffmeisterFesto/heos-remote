using heos_remote_lib;
using CommandLine;

namespace heos_remote_ctl
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            // parse options (do not care about errors)
            var result = Parser.Default.ParseArguments<HeosAppOptions>(args);
            if (result.Value == null)
                return -1;
            
            var options = result.Value;

            // say hello
            if (options.Verbose > 0) 
                Console.WriteLine("heos-remote-ctl (c) 2025 by Michael Hoffmeister, MIT license.");

            // find a device
            var device = (await HeosDiscovery.DiscoverItems(firstFriedlyName: options.GetDeviceTuples()?.FirstOrDefault()?.Item1, debugLevel: 2)).FirstOrDefault();
            if (device == null)
            {
                Console.Error.WriteLine("No device found. Aborting!");
                return -1;
            }

            // TelnetClient builds up a connection per send
            var tc = new HeosTelnet(device.Host);

            // find the player
            var o1 = await tc.SendCommandAsync("heos://player/get_players\r\n");
            if (!HeosTelnet.IsSuccessCode(o1))
            {
                Console.Error.WriteLine("heos://player/get_players returned with no success. Aborting!");
                return -1;
            }

            int? pid = null;
            foreach (var pay in o1.payload)
                if (pay.name.ToString() == options.Devices)
                    pid = pay.pid;

            if (!pid.HasValue)
            {
                Console.Error.WriteLine("Device not found in players list. Aborting!");
                return -1;
            }

            // get player state
            var o2 = await tc.SendCommandAsync($"heos://player/get_play_state?pid={pid}\r\n");
            if (!HeosTelnet.IsSuccessCode(o2))
            {
                Console.Error.WriteLine("heos://player/get_play_state returned with no success. Aborting!");
                return -1;
            }
            var playState = o2.heos.message;

            // pause
            var o3 = await tc.SendCommandAsync($"heos://player/set_play_state?pid={pid}&state={"play"}\r\n");
            if (!HeosTelnet.IsSuccessCode(o3))
            {
                Console.Error.WriteLine("heos://player/set_play_state returned with no success. Aborting!");
                return -1;
            }

            // ok?
            return 0;
        }
    }
}
