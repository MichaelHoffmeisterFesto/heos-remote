using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Action<int, HeosConnectedItem?>? lambdaSetPidPlayer = null,
            Func<string, Task>? lambdaMsg = null,
            Action<HeosDiscoveredItem?, List<Tuple<string, string>>?, string?>? lambdaInfoBox = null)
        {
            // access
            if (options == null || deviceConfig == null || ConnMgr == null || cmd?.HasContent() != true)
                return false;
            if (!("Toggle Play Pause Next Prev Fav 1 Fav 2 Fav 3 Aux In SPDIF In HDMI In Vol + Vol - Reboot Info Browse".Contains(cmd)))
                return false;

            // any command to the device

            // establish device
            var device = await ConnMgr.DiscoverOrGet(deviceConfig: deviceConfig, debugLevel: 0, androidMode: androidMode);
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
    }
}
