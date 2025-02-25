﻿using System;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace heos_remote_systray
{
    public class HeosCustomApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        private ContextMenuStrip contextMenu;

        private FormInfo2? formInfo = null;

        private HeosConnectedItemMgr ConnMgr = new();

        protected HeosContainerLocation _currentBrowseLocation = new HeosContainerLocation() { Name = "TuneIn", Sid = 3, Cid = "" };
        protected List<HeosContainerLocation> _currentHistory = new List<HeosContainerLocation>();

        protected ToolStripDropDownButton? _deviceDropDownButton = null;

        protected string _activeDeviceName = "XXX";

        public HeosCustomApplicationContext()
        {
            // some inits
            var startCnt = Options.Curr.GetStartPoints()?.FirstOrDefault();
            if (startCnt != null)
                _currentBrowseLocation = startCnt.Copy();

            // configure context menu
            contextMenu = new ContextMenuStrip()
            {
            };

            ////////

            _deviceDropDownButton = BuildContextMenuDropDown(
                "D:Schlafzimmer  \u25b6 ",
                Options.Curr.Devices.Select((dev) =>
                {
                    var (fn, ep) = HeosAppOptions.SplitDeviceName(dev);
                    return "" + fn;
                }).ToArray(),
                showDropDownArrow: false,
                onClick: contextMenuItemHandler);

            contextMenu.Items.Add(_deviceDropDownButton);

            ////////

            contextMenu.Items.Add("Play", image: WinFormsUtils.ByteToImage(Resources.heos_remote_play), 
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Pause", image: WinFormsUtils.ByteToImage(Resources.heos_remote_pause),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Next", image: WinFormsUtils.ByteToImage(Resources.heos_remote_next),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Prev", image: WinFormsUtils.ByteToImage(Resources.heos_remote_prev),
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
            contextMenu.Items.Add("Aux In", image: WinFormsUtils.ByteToImage(Resources.heos_remote_aux_in),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("SPDIF In", image: WinFormsUtils.ByteToImage(Resources.heos_remote_spdif_in),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("HDMI In", image: WinFormsUtils.ByteToImage(Resources.heos_remote_hdmi_in),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Browse", image: WinFormsUtils.ByteToImage(Resources.heos_remote_browse),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Play URL", image: WinFormsUtils.ByteToImage(Resources.heos_remote_url),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Info", image: WinFormsUtils.ByteToImage(Resources.heos_remote_info),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Exit", image: null, 
                onClick: contextMenuItemHandler);

            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = WinFormsUtils.BytesToIcon(Resources.heos_remote_icon_I5p_icon),
                Visible = true,
                ContextMenuStrip = contextMenu,
            };

            trayIcon.Click += new System.EventHandler(trayIcon_Click);
            trayIcon.DoubleClick += new System.EventHandler(trayIcon_DoubleClick);
        }

        protected ToolStripDropDownButton BuildContextMenuDropDown(
            string rootName,
            string[] items,
            EventHandler? onClick,
            bool showDropDownArrow = true)
        {
            var res = new ToolStripDropDownButton();
            var dropDown = new ToolStripDropDown() { DropShadowEnabled = false };
            res.Text = rootName;

            res.Margin = new Padding(2, 8, 2, 2);
            res.DropDown = dropDown;
            res.AutoToolTip = false;
            res.DropDownDirection = ToolStripDropDownDirection.Right;
            res.ShowDropDownArrow = showDropDownArrow;

            foreach (var item in items)
            {
                var itemButton = new ToolStripButton(text: item, image: null, onClick: onClick);
                dropDown.Items.Add(item);
            }

            return res;
        }

        async Task executeCommand(string cmd)
        {
            // check first
            if (!("Toggle Info Play Pause Next Prev Fav 1 Fav 2 Fav 3 Aux In SPDIF In HDMI In Vol + Vol - Play URL Browse".Contains(cmd)))
                return;

            // any command to the device

            // establish device
            // var device = (await HeosDiscovery.DiscoverItems(firstFriedlyName: Options.Curr.Device, debugLevel: 0)).FirstOrDefault();
            var device = await ConnMgr.DiscoverOrGet(friedlyName: Options.Curr.Devices ?? "", debugLevel: 0);
            if (device?.Telnet == null)
            {
                trayIcon.ShowBalloonTip(500, "HEOS Control", "No device found. Aborting!", ToolTipIcon.Info);
                return;
            }

            // find the player
            var o1 = await device.Telnet.SendCommandAsync("heos://player/get_players\r\n");
            if (!HeosTelnet.IsSuccessCode(o1))
            {
                trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/get_players returned with no success. Aborting!", ToolTipIcon.Info);
                return;
            }

            var (fn, ep) = HeosAppOptions.SplitDeviceName(Options.Curr.Devices);
            int? pid = null;
            foreach (var pay in o1.payload)
                if (pay.name.ToString() == fn)
                    pid = pay.pid;

            if (!pid.HasValue)
            {
                trayIcon.ShowBalloonTip(500, "HEOS Control", "Device not found in players list. Aborting!", ToolTipIcon.Info);
                return;
            }

            if (cmd == "Pause")
            {
                // pause
                var output = await device.Telnet.SendCommandAsync($"heos://player/set_play_state?pid={pid}&state={"pause"}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/set_play_state returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if (cmd == "Play")
            {
                // play
                var output = await device.Telnet.SendCommandAsync($"heos://player/set_play_state?pid={pid}&state={"play"}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/set_play_state returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if (cmd == "Toggle")
            {
                // get the state
                var output = await device.Telnet.SendCommandAsync($"heos://player/get_play_state?pid={pid}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/get_play_state returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }

                // decide next state
                bool isPlay = (output?.heos.message?.ToString() ?? "").Contains("play");
                var nextPlayState = isPlay ? "pause" : "play";

                // pause
                output = await device.Telnet.SendCommandAsync($"heos://player/set_play_state?pid={pid}&state={nextPlayState}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/set_play_state returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if (cmd == "Next")
            {
                // next
                var output = await device.Telnet.SendCommandAsync($"heos://player/play_next?pid={pid}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/play_next returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if (cmd == "Prev")
            {
                // previous
                var output = await device.Telnet.SendCommandAsync($"heos://player/play_previous?pid={pid}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/play_previous returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if (cmd == "Vol +")
            {
                // volume down
                var output = await device.Telnet.SendCommandAsync($"heos://player/volume_up?pid={pid}&step={5}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/volume_up returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if (cmd == "Vol -")
            {
                // volume down
                var output = await device.Telnet.SendCommandAsync($"heos://player/volume_down?pid={pid}&step={5}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
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
                var output = await device.Telnet.SendCommandAsync($"heos://system/sign_in?un={Options.Curr.Username}&pw={Options.Curr.Password}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://system/sign_in returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }

                // select
                output = await device.Telnet.SendCommandAsync($"heos://browse/play_preset?pid={pid}&preset={favNdx}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://browse/play_preset returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if ("Aux In SPDIF In HDMI In".Contains(cmd))
            {
                // index
                string input = "inputs/aux_in_1";
                if (cmd == "SPDIF In") input = "inputs/optical_in_1";
                if (cmd == "HDMI In") input = "inputs/hdmi_in_1";

                // select
                var output = await device.Telnet.SendCommandAsync($"heos://browse/play_input?pid={pid}&input={input}\r\n");
                if (!HeosTelnet.IsSuccessCode(output))
                {
                    trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://browse/play_input returned with no success. Aborting!", ToolTipIcon.Info);
                    return;
                }
            }

            if (cmd == "Info")
            {
                // ask for current
                List<Tuple<string, string>>? nowPlay = new();
                string imgUrl = null;
                try
                {
                    var output = await device.Telnet.SendCommandAsync($"heos://player/get_now_playing_media?pid={pid}\r\n");
                    if (!HeosTelnet.IsSuccessCode(output))
                    {
                        trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/get_now_playing_media returned with no success. Aborting!", ToolTipIcon.Info);
                        return;
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
                if (formInfo == null)
                    formInfo = new FormInfo2(devInfo: device.Item, nowPlay: nowPlay, urlForImage: imgUrl);
                formInfo.Show();
                formInfo.FormClosed += new FormClosedEventHandler(formInfoClosedEventHandler);
            }

            if (cmd == "Browse")
            {
                // build a list of starting points
                var genStarts = (await new HeosMusicSourceList().Acquire(device))?.GetStartPoints();
                var starts = Options.Curr.GetStartPoints();
                if (genStarts != null)
                    starts = starts.Union(genStarts);

                // give this to the browse
                var formBrowse = new FormContainerBrowser();
                formBrowse.Device = device;
                formBrowse.StartingPoints = starts?.ToList();
                formBrowse.CurrentLocation = _currentBrowseLocation.Copy();
                formBrowse.History = new List<HeosContainerLocation>(_currentHistory);

                formBrowse.LambdaLoadSongQueue = async () =>
                {
                    var sq = await new HeosSongQueue().Acquire(device, pid.Value);
                    return sq;
                };

                formBrowse.LambdaClearQueue = async () =>
                {
                    // execute
                    var o5 = await device.Telnet.SendCommandAsync($"heos://player/clear_queue?pid={pid}\r\n");
                    if (!HeosTelnet.IsSuccessCode(o5))
                    {
                        trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/clear_queue returned with no success. Aborting!", ToolTipIcon.Info);
                        return;
                    }
                };

                formBrowse.LambdaPlayItem = async (parentLoc, item, action) =>
                {
                    // access
                    if (action < 1 || action > 4)
                        return;
                    // which kind?
                    if (item == null && parentLoc != null)
                    {
                        // play the full container
                        var o7 = await device.Telnet.SendCommandAsync($"heos://browse/add_to_queue?pid={pid}&sid={parentLoc.Sid}&cid={parentLoc.Cid}&aid={action}\r\n");
                        if (!HeosTelnet.IsSuccessCode(o7))
                        {
                            trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://browse/add_to_queue returned with no success. Aborting!", ToolTipIcon.Info);
                            return;
                        }
                    }
                    else if (item?.IsContainer == true)
                    {
                        // add container to queue
                        var o5 = await device.Telnet.SendCommandAsync($"heos://browse/add_to_queue?pid={pid}&sid={parentLoc.Sid}&cid={item.Cid}\r\n");
                        if (!HeosTelnet.IsSuccessCode(o5))
                        {
                            trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/get_now_playing_media returned with no success. Aborting!", ToolTipIcon.Info);
                            return;
                        }

                    }
                    else if (item?.Mid?.HasContent() == true)
                    {
                        if (item.Type == "station")
                        {
                            // play the stream (is not a track)
                            var o5 = await device.Telnet.SendCommandAsync($"heos://browse/play_stream?pid={pid}&sid={parentLoc.Sid}&cid={parentLoc.Cid}&mid={item.Mid}&name={item.Name}\r\n");
                            if (!HeosTelnet.IsSuccessCode(o5))
                            {
                                trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://browse/play_stream returned with no success. Aborting!", ToolTipIcon.Info);
                                return;
                            }
                        }
                        else
                        {
                            // add track to queue
                            var o5 = await device.Telnet.SendCommandAsync($"heos://browse/add_to_queue?pid={pid}&sid={parentLoc.Sid}&cid={parentLoc.Cid}&mid={item.Mid}&aid={action}\r\n");
                            if (!HeosTelnet.IsSuccessCode(o5))
                            {
                                trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://browse/add_to_queue returned with no success. Aborting!", ToolTipIcon.Info);
                                return;
                            }
                        }
                    }
                };

                var res = formBrowse.ShowDialog();
                if (res == DialogResult.OK)
                {
                    // save actual location
                    _currentBrowseLocation = formBrowse.CurrentLocation.Copy();
                    _currentHistory = new List<HeosContainerLocation>(formBrowse.History);
                }
            }

            if (cmd == "Play URL")
            {
                using (var dlg = new FormPlaySearch())
                {
                    // get music sources
                    var ms1 = await new HeosMusicSourceList().Acquire(device, onlyValid: false);
                    var ms2 = await new HeosSearchCriteriaList().Acquire(device, sid: 13);

                    var o5 = await device.Telnet.SendCommandAsync($"heos://browse/search?sid={13}&search={"Sting"}\r\n");
                    if (!HeosTelnet.IsSuccessCode(o5))
                    {
                        trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://player/get_now_playing_media returned with no success. Aborting!", ToolTipIcon.Info);
                        return;
                    }

                    // perform dialog
                    if (contextMenu != null)
                    {
                        var cml = contextMenu.Location;
                        dlg.Location = new Point(cml.X - dlg.Width, cml.Y);
                        dlg.StartPosition = FormStartPosition.Manual;
                    }

                    var res = dlg.ShowDialog();
                    if (res != DialogResult.OK)
                        return;

                    // ok
                    if (dlg.ResultSource == "Internet radio" && dlg.ResultKind == "URL")
                    {
                        var output = await device.Telnet.SendCommandAsync($"heos://browse/play_stream?pid={pid}&url={dlg.ResultText}\r\n");
                        if (!HeosTelnet.IsSuccessCode(output))
                        {
                            trayIcon.ShowBalloonTip(500, "HEOS Control", "heos://browse/play_stream returned with no success. Aborting!", ToolTipIcon.Info);
                            return;
                        }
                    }
                }
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
#if __does_not_work_context_menu_is_sticiing
            // Get the current position of the mouse
            Point mousePosition = Control.MousePosition;

            // if (e is MouseEventArgs me)
            contextMenu.AutoClose = true;
            contextMenu.Show(mousePosition);
#endif
        }

        void formInfoClosedEventHandler(object? sender, EventArgs e)
        {
            formInfo = null;
        }

    }
}
