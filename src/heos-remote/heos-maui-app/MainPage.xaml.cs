using Microsoft.Maui.Controls.Platform;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using heos_remote_lib;
using heos_remote_systray;
using Newtonsoft.Json;
using Options = heos_maui_app.OptionsSingleton;

namespace heos_maui_app
{
    public partial class MainPage : ContentPage
    {
        protected HeosConnectedItemMgr _connMgr = new(); 
        
        protected HeosContainerLocation _currentBrowseLocation = new HeosContainerLocation() { Name = "TuneIn", Sid = 3, Cid = "" };
        protected HeosDeviceConfig? _activeDevice = null;

        protected List<HeosDeviceConfig> _deviceConfigs = new();
        protected List<HeosGroupConfig> _groupConfigs = new();

#if ANDROID
        protected bool _androidMode = true;
#else
        protected bool _androidMode = false;
#endif

        public class ButtonConfig
        {
            public string Fn { get; set; } = "";
            public string Cmd { get; set; } = "";
            public Button? Button { get; set; } = null;
            public ButtonConfig() { }
            public ButtonConfig(string fn, string cmd, Button? button)
            {
                Fn = fn;
                Cmd = cmd;
                Button = button;
            }
        }

        protected List<ButtonConfig> _buttonConfig = new List<ButtonConfig>()
        {
            new ButtonConfig("heos_remote_play.png", "Play", null),
            new ButtonConfig("heos_remote_pause.png", "Pause", null),
            new ButtonConfig("heos_remote_aux_in.png", "Aux In", null),
            new ButtonConfig("heos_remote_prev.png", "Prev", null),
            new ButtonConfig("heos_remote_next.png", "Next", null),
            new ButtonConfig("heos_remote_hdmi_in.png", "HDMI In", null),
            new ButtonConfig("heos_remote_vol_down.png", "Vol -", null),
            new ButtonConfig("heos_remote_vol_up.png", "Vol +", null),
            new ButtonConfig("heos_remote_reboot.png", "Reboot", null),
            new ButtonConfig("heos_remote_fav1.png", "Fav 1", null),
            new ButtonConfig("heos_remote_fav2.png", "Fav 2", null),
            new ButtonConfig("heos_remote_fav3.png", "Fav 3", null),
            new ButtonConfig("heos_remote_faders.png", "Faders", null),
            new ButtonConfig("heos_remote_power_on.png", "Power On", null),
            new ButtonConfig("heos_remote_power_off.png", "Power Off", null),
        };

        public MainPage()
        {
            InitializeComponent();
        }
       
        private void ContentPage_Loaded(object sender, EventArgs e)
        {
            // make up options
            Options.Curr.Devices = new List<string>(new[] { "Name1", "Name2" });

            // UI restart
            RestartFromOptions();

            // init buttons (grid rows)
            if (_buttonConfig.Count > 0)
            {
                ButtonGrid.RowDefinitions.Clear();
                for (int i = 0; i < (_buttonConfig.Count - 1) / 3 + 1; i++)
                    ButtonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            // init buttons (buttons itself)
            for (int i = 0; i < _buttonConfig.Count; i++)
            {
                if (_buttonConfig.Count <= i)
                    break;
                var bc = _buttonConfig[i];

                var btn = new Button
                {
                    Text = $"{bc.Cmd}",
                    ImageSource = bc.Fn,
                    ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 2),
                    AutomationId = $"Button{i}"
                };
                bc.Button = btn;

#if ANDROID
                var x = DeviceDisplay.Current.MainDisplayInfo.Height;
                btn.Padding = new Thickness(0, 1 + x / 50, 0, 0);
#endif

                btn.Background = Brush.LightGray;
                btn.CornerRadius = 2;
                btn.Margin = new Thickness(3);

                Grid.SetRow(btn, i / 3);
                Grid.SetColumn(btn, i % 3);

                btn.Clicked += async (s, e) =>
                {
                    // find button?
                    foreach (var bc in _buttonConfig)
                    {
                        if (bc.Button == s)
                        {
                            await executeCommand(bc.Cmd);
                        }
                    }
                };

                ButtonGrid.Children.Add(btn);
            }
        }

        protected void RestartFromOptions()
        {
            // load options?
            {
                var jsonFn = Path.Combine(FileSystem.AppDataDirectory, "heos-config.json");
                var testOptions = new HeosAppOptions();
                if (HeosAppOptions.ReadJson(jsonFn, testOptions))
                    Options.Curr = testOptions;
            }

            // some first inits
            var startCnt = Options.Curr.GetStartPoints()?.FirstOrDefault();
            if (startCnt != null)
                _currentBrowseLocation = startCnt.Copy();

            _connMgr = new();            

            _activeDevice = Options.Curr.GetDeviceConfigs()?.FirstOrDefault();

            // init device picker
            _deviceConfigs = Options.Curr.GetDeviceConfigs().ToList();
            
            DevicePicker.Items.Clear();
            foreach (var dc in _deviceConfigs)
                DevicePicker.Items.Add(dc.FriendlyName);
            if (DevicePicker.Items.Count > 0)
                DevicePicker.SelectedIndex = 0;

            // init group picker
            _groupConfigs = new();
            if (Options.Curr.Groups != null)
                foreach (var gcstr in Options.Curr.Groups)
                {
                    var gc = new HeosGroupConfig(gcstr, _deviceConfigs.Count);
                    if (gc.IsValid())
                        _groupConfigs.Add(gc);
                }

            GroupPicker.Items.Clear();
            foreach (var gc in _groupConfigs)
                if (gc.IsValid())
                    GroupPicker.Items.Add(gc.Name);
        }

        private async void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender == DevicePicker)
            {
                var i = DevicePicker.SelectedIndex;
                if (_deviceConfigs == null || i < 0 || i >= _deviceConfigs.Count)
                    return;
                _activeDevice = _deviceConfigs[i];
            }

            if (sender == GroupPicker)
            {
                var i = GroupPicker.SelectedIndex;
                if (_groupConfigs == null || i < 0 || i >= _groupConfigs.Count())
                    return;

                // get the device
                var device = await _connMgr.DiscoverOrGet(
                    deviceConfig: _activeDevice,
                    androidMode: _androidMode,
                    interfaceName: Options.Curr.IfcName,
                    debugLevel: 0);
                if (device?.Telnet == null)
                    return;

                // do it
                int nextDevToSelect = -1;
                await _groupConfigs[i].Execute(_deviceConfigs, device, unGroup: true,
                    setDevIndexAsMaster: (i) =>
                    {
                        if (nextDevToSelect < 0 && i >=0 && i < _deviceConfigs.Count)
                            nextDevToSelect = i;
                    });

                // (visually) re-select the device
                if (nextDevToSelect >= 0 && nextDevToSelect < _deviceConfigs.Count)
                {
                    DevicePicker.SelectedIndex = nextDevToSelect;
                    _activeDevice = _deviceConfigs[nextDevToSelect];
                }
            }
        }

        async Task executeCommand(string cmd)
        {
            // access
            int? gotPid = null;
            HeosConnectedItem? gotDevice = null;

            // be safe 
            try
            {

                // in case of some commands, we want to check back with the user first
                if ("Reboot".Contains(cmd))
                {
                    bool askBackResult = await DisplayAlert("Confirmation", $"Do you want to execute the command: {cmd} ?", "Yes", "No");
                    if (askBackResult != true)
                        return;
                }

                if ("Faders".Contains(cmd))
                {
                    await Button_Special_Clicked(cmd);
                    return;
                }

                // try directly to refer to library command handling,
                // this will activate some lambdas
                await HeosCommands.ExecuteSimpleCommand(
                    options: Options.Curr,
                    ConnMgr: _connMgr,
                    deviceConfig: _activeDevice,
                    cmd: cmd,
                    androidMode: _androidMode,
                    interfaceName: Options.Curr.IfcName,
                    lambdaSetPidPlayer: (pid, device) =>
                    {
                        gotPid = pid;
                        gotDevice = device;
                    },
                    lambdaMsg: async (msg) =>
                    {
                        await MauiUiHelper.ShowToast(msg);
                    },
                    lambdaInfoBox: (dev, nowPlay, imgUrl) =>
                    {
                        ;
                    });

                // further commands possible?
                if (gotPid.HasValue && gotDevice?.Telnet != null)
                {
                    if (cmd == "Browse")
                    {
                    }

                    if (cmd == "Play URL")
                    {
                    }
                }
            } 
            catch (Exception ex)
            {
                await MauiUiHelper.ShowToast($"Executing command {cmd} gave exception: {ex.Message} in {ex.StackTrace}");
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if (sender == ButtonSetup)
            {
                var setupPage = new SetupPage();

                setupPage.Text = JsonConvert.SerializeObject(Options.Curr, Formatting.Indented);

                setupPage.Disappearing += async (s2, e2) => {
                    if (setupPage.Result)
                    {
                        HeosAppOptions? newOpt = null;
                        try
                        {
                            newOpt = JsonConvert.DeserializeObject<HeosAppOptions>(setupPage.Text);
                        }
                        catch
                        {
                            newOpt = null;
                        }
                        
                        if (newOpt == null)
                            await MauiUiHelper.ShowToast("Error converting JSON to options!");
                        else
                            Options.Curr = newOpt;

                        // write it to the save file
                        var jsonFn = Path.Combine(FileSystem.AppDataDirectory, "heos-config.json");
                        if (!HeosAppOptions.WriteJson(jsonFn, Options.Curr))
                        {
                            await MauiUiHelper.ShowToast("Error writing JSON options to user config file!");
                            return;
                        }

                        // UI restart
                        RestartFromOptions();
                    }
                };

                await Navigation.PushModalAsync(setupPage);
            }
        }

        private async Task Button_Special_Clicked(string cmd)
        {
            if (cmd == "Faders")
            {
                // makes only sense with pids found
                var pidsFound = await HeosCommands.ExecuteCheckForPids(
                    _deviceConfigs, 
                    _activeDevice,
                    androidMode: _androidMode,
                    interfaceName: Options.Curr.IfcName,
                    connMgr: _connMgr);
                if (pidsFound < 1)
                {
                    await MauiUiHelper.ShowToast("No player ids found for active device!");
                    return;
                }

                // which faders
                var fadersNdx = SplitStringOfDeviceIndices(Options.Curr.Faders);
                if (fadersNdx.Count < 1)
                {
                    await MauiUiHelper.ShowToast("No options \"Faders\" configured!");
                    return;
                }

                var fadersPage = new FadersPage(_deviceConfigs, fadersNdx,
                    faderGetVolume: async (dev) =>
                    {
                        var vol = await HeosCommands.ExecuteGetVolumeForPid(
                                    _deviceConfigs,
                                    _activeDevice,
                                    androidMode: _androidMode,
                                    interfaceName: Options.Curr.IfcName,
                                    connMgr: _connMgr,
                                    pid: dev.Pid?? "");

                        if (vol.HasValue)
                            return vol.Value;
                        else
                            return null;
                    },
                    faderSetVolume: async (dev, vd) =>
                    {
                        int vi = (int)vd;
                        var res = await HeosCommands.ExecuteSetVolumeForPid(
                                    _deviceConfigs,
                                    _activeDevice,
                                    androidMode: _androidMode,
                                    interfaceName: Options.Curr.IfcName,
                                    connMgr: _connMgr,
                                    pid: dev.Pid ?? "",
                                    volume: vi);

                        return res;
                    },
                    faderGetInfo: async (dev) =>
                    {
                        var pi = await HeosCommands.ExecuteGetPlayingInfoForPid(
                                    _deviceConfigs,
                                    _activeDevice,
                                    androidMode: _androidMode,
                                    interfaceName: Options.Curr.IfcName,
                                    connMgr: _connMgr,
                                    pid: dev.Pid ?? "",
                                    checkPlayModeAsWell: true,
                                    lambdaMapInputToUrl: (mid) => {
                                        if (mid?.HasContent() != true)
                                            return "";
                                        if (mid.StartsWith("inputs/hdmi"))
                                            return "heos_remote_hdmi_in.png";
                                        if (mid.StartsWith("inputs/aux"))
                                            return "heos-remote-aux-in.png";
                                        if (mid.StartsWith("inputs/optical"))
                                            return "heos_remote_spdif_in.png";
                                        return "";
                                    });

                        return pi;
                    },
                    lambdaFunctionSelected: async (dev,tag) =>
                    {
                        await Task.Yield();

                        if (dev != null && tag is string cmd 
                            && cmd.HasContent() == true && "Play Pause Prev Next".Contains(cmd))
                        {
                            // be safe 
                            try
                            {
                                // try directly to refer to library command handling,
                                // this will activate some lambdas
                                await HeosCommands.ExecuteSimpleCommand(
                                    options: Options.Curr,
                                    ConnMgr: _connMgr,
                                    deviceConfig: dev,
                                    cmd: cmd,
                                    androidMode: _androidMode,
                                    interfaceName: Options.Curr.IfcName,
                                    lambdaMsg: async (msg) =>
                                    {
                                        await MauiUiHelper.ShowToast(msg);
                                    });
                            }
                            catch (Exception ex)
                            {
                                await MauiUiHelper.ShowToast($"Executing command {cmd} gave exception: {ex.Message} in {ex.StackTrace}");
                            }
                        }

                        if (tag as string == "Select")
                        {
                            // find device id
                            int foundDi = -1;
                            foreach (var d in _deviceConfigs)
                                if (d.FriendlyName.Equals(dev?.FriendlyName ?? "", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    foundDi = _deviceConfigs.IndexOf(d);
                                    break;
                                }
                            if (foundDi < 0)
                                return;

                            // select on picker .. will do the rest
                            if (foundDi < DevicePicker.Items.Count)
                                DevicePicker.SelectedIndex = foundDi;
                        }
                    });

                fadersPage.Disappearing += async (s2, e2) => {
                    await Task.Yield();
                    if (fadersPage.Result)
                    {
                    }
                };

                await Navigation.PushModalAsync(fadersPage);
            }
        }

        protected List<int> SplitStringOfDeviceIndices(string str)
        {
            var res = new List<int>();
            if (str?.HasContent() != true)
                return res;
            var inner = str.Split(",");
            foreach (var istr in inner)
            {
                if (!int.TryParse(istr, out int i)
                    || i < 1 || i > _deviceConfigs.Count)
                    continue;
                res.Add(i-1);
            }
            return res;
        }
    }

}
