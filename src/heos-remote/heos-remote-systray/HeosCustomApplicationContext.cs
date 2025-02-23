using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using heos_remote_lib;
using Options = heos_remote_systray.OptionsSingleton;
using static System.Windows.Forms.Design.AxImporter;
using Newtonsoft.Json.Linq;

namespace heos_remote_systray
{
    public class HeosCustomApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        private ContextMenuStrip contextMenu;

        private FormInfo? formInfo = null;

        public HeosCustomApplicationContext()
        {
            // Initialize Tray Icon
            contextMenu = new ContextMenuStrip()
            {
            };
            
            contextMenu.Items.Add("Play", image: WinFormsUtils.ByteToImage(Resources.heos_remote_play), 
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Pause", image: WinFormsUtils.ByteToImage(Resources.heos_remote_pause),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Vol +", image: WinFormsUtils.ByteToImage(Resources.heos_remote_vol_up),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Vol -", image: WinFormsUtils.ByteToImage(Resources.heos_remote_vol_down),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Fav 1", image: WinFormsUtils.ByteToImage(Resources.heos_remote_fav1),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Fav 2", image: WinFormsUtils.ByteToImage(Resources.heos_remote_fav2),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Fav 3", image: WinFormsUtils.ByteToImage(Resources.heos_remote_fav3),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Info", image: null,
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Exit", image: null, 
                onClick: contextMenuItemHandler);

            trayIcon = new NotifyIcon()
            {
                Icon = WinFormsUtils.BytesToIcon(Resources.heos_remote_icon_I5p_icon),
                Visible = true,
                ContextMenuStrip = contextMenu,
            };

            trayIcon.Click += new System.EventHandler(trayIcon_Click);
            trayIcon.DoubleClick += new System.EventHandler(trayIcon_DoubleClick);
        }

        async Task executeCommand(string cmd)
        {
            // check first
            if (!("Toggle Info Play Pause Fav 1 Fav 2 Fav 3 Vol + Vol -".Contains(cmd)))
                return;

            // any command to the device

            // establish device
            var device = (await HeosDiscovery.DiscoverItems(firstFriedlyName: Options.Curr.Device, debugLevel: 0)).FirstOrDefault();
            if (device == null)
            {
                trayIcon.ShowBalloonTip(500, "HEOS Control", "No device found. Aborting!", ToolTipIcon.Info);
                return;
            }

            // TelnetClient builds up a connection per send
            var tc = new TelnetClient(device.Host, 1255);

            // find the player
            var o1 = await tc.SendCommandAsync("heos://player/get_players\r\n");
            if (o1?.heos.result.ToString() != "success")
            {
                trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/get_players returned with no success. Aborting!", ToolTipIcon.Info);
                return;
            }

            int? pid = null;
            foreach (var pay in o1.payload)
                if (pay.name.ToString() == Options.Curr.Device)
                    pid = pay.pid;

            if (!pid.HasValue)
            {
                trayIcon.ShowBalloonTip(500, "HEOS Control", "Device not found in players list. Aborting!", ToolTipIcon.Info);
                return;
            }

            if (cmd == "Pause")
            {
                // pause
                var output = await tc.SendCommandAsync($"heos://player/set_play_state?pid={pid}&state={"pause"}\r\n");
                if (output?.heos.result.ToString() != "success")
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/set_play_state returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if (cmd == "Play")
            {
                // pause
                var output = await tc.SendCommandAsync($"heos://player/set_play_state?pid={pid}&state={"play"}\r\n");
                if (output?.heos.result.ToString() != "success")
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/set_play_state returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if (cmd == "Toggle")
            {
                // get the state
                var output = await tc.SendCommandAsync($"heos://player/get_play_state?pid={pid}\r\n");
                if (output?.heos.result.ToString() != "success")
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/get_play_state returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }

                // decide next state
                bool isPlay = (output?.heos.message?.ToString() ?? "").Contains("play");
                var nextPlayState = isPlay ? "pause" : "play";

                // pause
                output = await tc.SendCommandAsync($"heos://player/set_play_state?pid={pid}&state={nextPlayState}\r\n");
                if (output?.heos.result.ToString() != "success")
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/set_play_state returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if (cmd == "Vol +")
            {
                // volume down
                var output = await tc.SendCommandAsync($"heos://player/volume_up?pid={pid}&step={5}\r\n");
                if (output?.heos.result.ToString() != "success")
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/volume_up returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if (cmd == "Vol -")
            {
                // volume down
                var output = await tc.SendCommandAsync($"heos://player/volume_down?pid={pid}&step={5}\r\n");
                if (output?.heos.result.ToString() != "success")
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/volume_down returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if ("Fav 1 Fav 2 Fav 3".Contains(cmd))
            {
                // index
                int favNdx = 1;
                if (cmd == "Fav 2") favNdx = 2;
                if (cmd == "Fav 3") favNdx = 3;

                // extra check
                if (Options.Curr.Username?.HasContent() != true
                    || Options.Curr.Password?.HasContent() != true)
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "Username/ password of the HEOS account need to be given. Aborting!", ToolTipIcon.Info);
                    return;
                }

                // check in
                var output = await tc.SendCommandAsync($"heos://system/sign_in?un={Options.Curr.Username}&pw={Options.Curr.Password}\r\n");
                if (output?.heos.result.ToString() != "success")
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://system/sign_in returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }

                // select
                output = await tc.SendCommandAsync($"heos://browse/play_preset?pid={pid}&preset={favNdx}\r\n");
                if (output?.heos.result.ToString() != "success")
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://browse/play_preset returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if (cmd == "Info")
            {
                // ask for current
                List<Tuple<string, string>>? nowPlay = new();
                try
                {
                    var output = await tc.SendCommandAsync($"heos://player/get_now_playing_media?pid={pid}\r\n");
                    if (output?.heos.result.ToString() != "success")
                    {
                        trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/get_now_playing_media returned with no success. Aborting!", ToolTipIcon.Info);
                        return;
                    }

                    if (output?.payload is JObject jpay)
                        foreach (var x in jpay)
                        {
                            nowPlay.Add(new Tuple<string, string>(x.Key, "" + x.Value?.ToString()));
                        }
                }
                catch { }

                // put this into the info
                if (formInfo == null)
                    formInfo = new FormInfo(devInfo: device, nowPlay: nowPlay);
                formInfo.Show();
                formInfo.FormClosed += new FormClosedEventHandler(formInfoClosedEventHandler);
            }
        }

        async void contextMenuItemHandler(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                // check for command
                await executeCommand(tsmi.Text ?? "XXX");

                if (tsmi.Text == "Exit")
                {
                    // Hide tray icon, otherwise it will remain shown until user mouses over it
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                    Application.Exit();
                }
            }
        }

        async void trayIcon_DoubleClick(object? sender, EventArgs e)
        {
            await executeCommand("Toggle");
        }

        void trayIcon_Click(object? sender, EventArgs e)
        {
            // Get the current position of the mouse
            Point mousePosition = Control.MousePosition;

            // if (e is MouseEventArgs me)
            contextMenu.AutoClose = true;
            contextMenu.Show(mousePosition);
        }

        void formInfoClosedEventHandler(object? sender, EventArgs e)
        {
            formInfo = null;
        }

    }
}
