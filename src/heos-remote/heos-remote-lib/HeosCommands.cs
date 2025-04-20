using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace heos_remote_lib
{
    public static class HeosCommands
    {
        public static async Task<bool> ExecuteSimpleCommand(
            HeosAppOptions options,
            HeosConnectedItemMgr? ConnMgr,            
            HeosDeviceConfig? deviceConfig,
            string cmd,
            bool androidMode = false,
            string? interfaceName = null,
            Action<int, HeosConnectedItem?>? lambdaSetPidPlayer = null,
            Func<string, Task>? lambdaMsg = null,
            Action<HeosDiscoveredItem?, List<Tuple<string, string>>?, string?>? lambdaInfoBox = null)
        {
            // access
            if (options == null || deviceConfig == null || ConnMgr == null || cmd?.HasContent() != true)
                return false;
            if (!("Toggle Play Pause Next Prev Fav 1 Fav 2 Fav 3 Aux In SPDIF In HDMI In Vol + Vol - Reboot Info Browse Power On Power Off".Contains(cmd)))
                return false;

            // any command to the device

            // establish device
            var device = await ConnMgr.DiscoverOrGet(deviceConfig: deviceConfig, debugLevel: 0, 
                            androidMode: androidMode, interfaceName: interfaceName);
            if (device?.Telnet == null)
            {
                lambdaMsg?.Invoke("No device found. Aborting!");
                return false;
            }

            // find the player
            var o1 = await device.Telnet.SendCommandAsync("heos://player/get_players\r\n");
            if (!HeosTelnet.IsSuccessCode(o1))
            {
                if (lambdaMsg != null)
                    await lambdaMsg.Invoke("heos://player/get_players returned with no success. Aborting!");
                return false;
            }

            // find it in the reported players list!!
            int? pid = null;
            foreach (var pay in o1.payload)
                if (deviceConfig.FriendlyName != null &&
                    deviceConfig.FriendlyName.Equals(pay.name.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    pid = pay.pid;

            if (!pid.HasValue)
            {
                if (lambdaMsg != null)
                    await lambdaMsg.Invoke("Device not found in players list. Aborting!");
                return false;
            }

            lambdaSetPidPlayer?.Invoke(pid.Value, device);

            if (cmd == "Pause")
            {
                // pause
                var output = await device.Telnet.SendCommandAsync($"heos://player/set_play_state?pid={pid}&state={"pause"}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke("heos://player/set_play_state returned with no success. Aborting!");
                    return false;
                }
                return true;
            }

            if (cmd == "Play")
            {
                // play
                var output = await device.Telnet.SendCommandAsync($"heos://player/set_play_state?pid={pid}&state={"play"}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke("heos://player/set_play_state returned with no success. Aborting!");
                    return false;
                }
                return true;
            }

            if (cmd == "Toggle")
            {
                // get the state
                var output = await device.Telnet.SendCommandAsync($"heos://player/get_play_state?pid={pid}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke("heos://player/get_play_state returned with no success. Aborting!");
                    return false;
                }

                // decide next state
                bool isPlay = (output?.heos.message?.ToString() ?? "").Contains("play");
                var nextPlayState = isPlay ? "pause" : "play";

                // pause
                output = await device.Telnet.SendCommandAsync($"heos://player/set_play_state?pid={pid}&state={nextPlayState}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke("heos://player/set_play_state returned with no success. Aborting!");
                    return false;
                }
                return true;
            }

            if (cmd == "Next")
            {
                // next
                var output = await device.Telnet.SendCommandAsync($"heos://player/play_next?pid={pid}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke("heos://player/play_next returned with no success. Aborting!");
                    return false;
                }
                return true;
            }

            if (cmd == "Prev")
            {
                // previous
                var output = await device.Telnet.SendCommandAsync($"heos://player/play_previous?pid={pid}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke("heos://player/play_previous returned with no success. Aborting!");
                    return false;
                }
                return true;
            }

            if (cmd == "Vol +")
            {
                // volume down
                var output = await device.Telnet.SendCommandAsync($"heos://player/volume_up?pid={pid}&step={5}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke("heos://player/volume_up returned with no success. Aborting!");
                    return false;
                }
                return true;
            }

            if (cmd == "Vol -")
            {
                // volume down
                var output = await device.Telnet.SendCommandAsync($"heos://player/volume_down?pid={pid}&step={5}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke("heos://player/volume_down returned with no success. Aborting!");
                    return false;
                }
                return true;
            }

            if (cmd == "Reboot")
            {
                // volume down
                var output = await device.Telnet.SendCommandAsync($"heos://system/reboot\r\n");
                // waiting makes no sense?!
                if (false && !HeosTelnet.IsSuccessCode(output))
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke("heos://system/reboot returned with no success. Aborting!");
                    return false;
                }
                return true;
            }

            if ("Fav 1 Fav 2 Fav 3".Contains(cmd))
            {
                // index
                int favNdx = 1;
                if (cmd == "Fav 2") favNdx = 2;
                if (cmd == "Fav 3") favNdx = 3;

                // extra check
                if (options.Username?.HasContent() != true
                    || options.Password?.HasContent() != true)
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke("Username/ password of the HEOS account need to be given. Aborting!");
                    return false;
                }

                // check in
                var output = await device.Telnet.SendCommandAsync($"heos://system/sign_in?un={options.Username}&pw={options.Password}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke("heos://system/sign_in returned with no success. Aborting!");
                    return false;
                }

                // select
                output = await device.Telnet.SendCommandAsync($"heos://browse/play_preset?pid={pid}&preset={favNdx}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke("heos://browse/play_preset returned with no success. Aborting!");
                    return false;
                }
                return true;
            }

            if ("Aux In SPDIF In HDMI In".Contains(cmd))
            {
                // index
                string input = "inputs/aux_in_1";
                if (cmd == "SPDIF In") input = "inputs/optical_in_1";
                if (cmd == "HDMI In") input = "inputs/hdmi_arc_1";

                // select
                var output = await device.Telnet.SendCommandAsync($"heos://browse/play_input?pid={pid}&input={input}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke("heos://browse/play_input returned with no success. Aborting!");
                    return false;
                }
                return true;
            }

            if ("Power On Power Off".Contains(cmd))
            {
                // for power commands, this seems NOT be available on the HEOS CLI, but only
                // on the Denon telnet command line interface. Make an ALTERNATE Telnet connection.
                var altTel = new HeosTelnet(device.Item.Host, port: 23);

                // peek if there is an receiver?
                // if corrent, the Amp shall reply with the status; this status is required do only
                // the valid switching
                bool? isState = null;
                var peekRes = await altTel.SendCommandAsync($"PW?\r", readTimeout: 1000, readToAnyDelim: true, readAnyText: true);
                if (peekRes?.ToUpper() == "PWON")
                    isState = true;
                if (peekRes?.ToUpper() == "PWSTANDBY")
                    isState = false;
                if (!isState.HasValue)
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke($"Device is not responding to power commands on port 23. Aborting!");
                    return false;
                }

                // target state?
                bool targetState = cmd == "Power On" ? true : false;
                if (isState.Value == targetState)
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke($"Device is already in the requested power state: {cmd}. Aborting!");
                    return false;
                }

                // try send command
                var targetCmd = targetState ? "PWON" : "PWSTANDBY";
                var res = await altTel.SendCommandAsync($"{targetCmd}\r\n", readTimeout: 1000, readToAnyDelim: true, readAnyText: true);
                if (!(res is string resstr)
                    || resstr.Trim().ToUpper() != targetCmd)
                {
                    if (lambdaMsg != null)
                        await lambdaMsg.Invoke($"Device did not reply properly to power command. Aborting!");
                    return false;
                }

                return true;
            }

            if (cmd == "Info")
            {
                // ask for current
                List<Tuple<string, string>>? nowPlay = new();
                string imgUrl = "";
                try
                {
                    var output = await device.Telnet.SendCommandAsync($"heos://player/get_now_playing_media?pid={pid}\r\n");
                    if (!HeosTelnet.IsSuccessCode(output))
                    {
                        if (lambdaMsg != null)
                            await lambdaMsg.Invoke("heos://player/get_now_playing_media returned with no success. Aborting!");
                        return false;
                    }

                    if (output?.payload is JObject jpay)
                        foreach (var x in jpay)
                        {
                            nowPlay.Add(new Tuple<string, string>(x.Key, "" + x.Value?.ToString()));
                            if (x.Key == "image_url")
                                imgUrl = x.Value?.ToString() ?? "";
                        }
                }
                catch { }

                // put this into the info
                lambdaInfoBox?.Invoke(device.Item, nowPlay, imgUrl);
                return true;
            }

            // uups?
            return false;
        }

        /// <returns>Number of devices with PID set.</returns>
        public static async Task<int> ExecuteCheckForPids(
            List<HeosDeviceConfig> deviceConfig,
            HeosDeviceConfig? activeDevice,
            HeosConnectedItemMgr? connMgr,
            bool androidMode = false,
            string? interfaceName = null)
        {
            // access
            if (connMgr == null)
                return 0;

            // establish device
            var foundDevice = await connMgr.DiscoverOrGet(deviceConfig: activeDevice, debugLevel: 0, 
                                androidMode: androidMode, interfaceName: interfaceName);
            if (foundDevice?.Telnet == null)
                return 0;

            // find all players
            var o1 = await foundDevice.Telnet.SendCommandAsync("heos://player/get_players\r\n");
            if (!HeosTelnet.IsSuccessCode(o1))
                return 0;

            // foreach device
            int res = 0;
            foreach (var dev in deviceConfig)
            {
                // already pid
                if (dev.Pid != null)
                {
                    res++;
                    continue;
                }

                // find?
                foreach (var pay in o1.payload)
                    if (dev.FriendlyName != null &&
                        dev.FriendlyName.Equals(pay.name.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        dev.Pid = pay.pid;

                // found?
                if (dev.Pid != null)
                    res++;
            }

            // ok 
            return res;
        }

        /// <returns>Volume between 0..100.</returns>
        public static async Task<double?> ExecuteGetVolumeForPid(
            List<HeosDeviceConfig> deviceConfig,
            HeosDeviceConfig? activeDevice,
            HeosConnectedItemMgr? connMgr,
            string pid,
            bool androidMode = false,
            string? interfaceName = null)
        {
            // access
            if (connMgr == null || pid.HasContent() != true)
                return null;

            // establish device
            var foundDevice = await connMgr.DiscoverOrGet(deviceConfig: activeDevice, debugLevel: 0, 
                                androidMode: androidMode, interfaceName: interfaceName);
            if (foundDevice?.Telnet == null)
                return null;

            // find volume
            var o1 = await foundDevice.Telnet.SendCommandAsync($"heos://player/get_volume?pid={pid}\r\n");
            if (!HeosTelnet.IsSuccessCode(o1))
                return null;

            // try extracting the volume
            var msg = o1?.heos.message?.ToString();
            if (msg== null || msg == "")
                return 0;
            var match = Regex.Match(msg, @"level=([0-9.]+)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return 0;
            if (!double.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.Number, CultureInfo.InvariantCulture, out double vol))
                return 0;

            // ok 
            return vol;
        }

        /// <summary>
        /// Set the volume for a given PID.
        /// </summary>
        /// <param name="deviceConfig">Static configuration of device names.</param>
        /// <param name="activeDevice">The device to issue the commands to.</param>
        /// <param name="connMgr">Manager to find devices.</param>
        /// <param name="pid">Player id</param>
        /// <param name="androidMode"></param>
        /// <param name="volume">Volume between 0..100</param>
        /// <returns>Success</returns>
        public static async Task<bool> ExecuteSetVolumeForPid(
            List<HeosDeviceConfig> deviceConfig,
            HeosDeviceConfig? activeDevice,
            HeosConnectedItemMgr? connMgr,
            string pid,
            int volume,
            bool androidMode = false,
            string? interfaceName = null)
        {
            // access
            if (connMgr == null || pid.HasContent() != true || volume < 0 || volume > 100)
                return false;

            // establish device
            var foundDevice = await connMgr.DiscoverOrGet(deviceConfig: activeDevice, debugLevel: 0, 
                                androidMode: androidMode, interfaceName: interfaceName);
            if (foundDevice?.Telnet == null)
                return false;

            // set volume
            var o1 = await foundDevice.Telnet.SendCommandAsync($"heos://player/set_volume?pid={pid}&level={volume}\r\n");
            if (!HeosTelnet.IsSuccessCode(o1))
                return false;

            // ok
            return true;
        }

        public static string ResolveSid(int sid)
        {
            switch (sid)
            {
                case 1: return "Pandora";
                case 2: return "Rhapsody";
                case 3: return "TuneIn";
                case 4: return "Spotify";
                case 5: return "Deezer";
                case 6: return "Napster";
                case 7: return "iHeartRadio";
                case 8: return "Sirius XM";
                case 9: return "Soundcloud";
                case 10: return "Tidal";
                case 13: return "Amazon Music";
                case 15: return "Moodmix";
                case 18: return "QQMusic";
                default: return ""; 
            }
        }

        public static async Task<HeosPlayingInfo?> ExecuteGetPlayingInfoForPid(
            List<HeosDeviceConfig> deviceConfig,
            HeosDeviceConfig? activeDevice,
            HeosConnectedItemMgr? connMgr,
            string pid,
            bool checkPlayModeAsWell = false,
            bool androidMode = false,
            string? interfaceName = null,
            Func<string, string>? lambdaMapInputToUrl = null)
        {
            // access
            if (connMgr == null || pid.HasContent() != true)
                return null;

            // establish device
            var foundDevice = await connMgr.DiscoverOrGet(deviceConfig: activeDevice, debugLevel: 0, 
                                androidMode: androidMode, interfaceName: interfaceName);
            if (foundDevice?.Telnet == null)
                return null;

            // result
            var res = new HeosPlayingInfo()
            {
                IsPlaying = false
            };

            // play mode
            if (checkPlayModeAsWell)
            {
                // find play state
                var o1 = await foundDevice.Telnet.SendCommandAsync($"heos://player//get_play_state?pid={pid}\r\n");
                if (HeosTelnet.IsSuccessCode(o1))
                {
                    var msg = o1?.heos.message?.ToString();
                    var match = Regex.Match(msg, @"state=(\w+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                        res.IsPlaying = match.Groups[1].Value.Equals("play", StringComparison.InvariantCultureIgnoreCase);
                }
            }

            // main stuff
            if (true)
            {
                // find now playing media
                var o1 = await foundDevice.Telnet.SendCommandAsync($"heos://player/get_now_playing_media?pid={pid}\r\n");

                if (HeosTelnet.IsSuccessCode(o1))
                {
                    var typ = o1?.payload?.type.ToString();
                    var sta = o1?.payload?.station.ToString();
                    var sng = o1?.payload?.song.ToString();
                    var alb = o1?.payload?.album.ToString();
                    var abi = o1?.payload?.album_id.ToString();
                    var art = o1?.payload?.artist.ToString();
                    var mid = o1?.payload?.mid.ToString();
                    var url = o1?.payload?.image_url.ToString();
                    int sid = (o1?.payload?.sid ?? 0);

                    if (abi == "inputs/")
                    {
                        res.FirstLine = "" + sta;
                        res.Nextlines = "" + mid;

                        if (lambdaMapInputToUrl != null)
                            res.Url = lambdaMapInputToUrl(mid);
                    }
                    else
                    if (typ == "song")
                    {
                        res.FirstLine = string.Join(" - ", ResolveSid(sid), sng);
                        res.Nextlines = string.Join(": ", "" + art, alb);
                        res.Url = "" + url;
                    }
                    else
                    {
                        res.FirstLine = string.Join(" - ", ResolveSid(sid), sta);
                        res.Nextlines = string.Join(" - ", "" + art, "" + alb);
                        res.Url = "" + url;
                    }
                }
            }

            // ok 
            return res;
        }

    }
}
