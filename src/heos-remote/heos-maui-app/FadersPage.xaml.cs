using CommunityToolkit.Maui.Views;
using heos_remote_lib;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using System.Collections.Generic;

namespace heos_maui_app;

public partial class FadersPage : ContentPage
{
    public bool Result { get; set; } = false;

    protected class FaderInfo
    {
        public HeosDeviceConfig Config;
        public Slider? Slider;
        public Image? Image;
        public ScrollView? Scroll;

        public FaderInfo(HeosDeviceConfig config)
        {
            Config = config;
        }
    }

    //protected List<HeosDeviceConfig> _deviceConfigs;
    //protected List<int> _faderIndices;

    protected List<FaderInfo> _faders;

    protected Func<HeosDeviceConfig, Task<double?>>? _faderGetVolume = null;
    protected Func<HeosDeviceConfig, double, Task<bool>>? _faderSetVolume = null;
    protected Func<HeosDeviceConfig, Task<HeosPlayingInfo?>>? _faderGetInfo = null;

    protected bool _goValueChanges = true;

    protected Dictionary<HeosDeviceConfig, HeosPlayingInfo?> _playingInfos = new();

    protected DateTime _lastSliderUpdate = DateTime.UtcNow;

    public FadersPage(
        List<HeosDeviceConfig> deviceConfigs, 
        List<int> faderIndices,
        Func<HeosDeviceConfig, Task<double?>>? faderGetVolume = null,
        Func<HeosDeviceConfig, double, Task<bool>>? faderSetVolume = null,
        Func<HeosDeviceConfig, Task<HeosPlayingInfo?>>? faderGetInfo = null)
	{
        _faders = new ();
        foreach (var i in faderIndices)
        {
            if (i < 0 || i >= deviceConfigs.Count)
                continue;
            _faders.Add(new FaderInfo(deviceConfigs[i]));
        }
        //_deviceConfigs = deviceConfigs;
        //_faderIndices = faderIndices;
        _faderGetVolume = faderGetVolume;
        _faderSetVolume = faderSetVolume;
        _faderGetInfo = faderGetInfo;
        InitializeComponent();
	}

    protected Popup? _myPopup = null;

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        await Task.Yield();

        // re-define fader grid rows
        FadersGrid.RowDefinitions.Clear();
        for (int i = 0; i < 3 * _faders.Count; i++)
        {
            FadersGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

        // re-define fader grid
        FadersGrid.Children.Clear();
        for (int i = 0; i < _faders.Count; i++)
        {
            var lab = new Label()
            {
                Text = _faders[i].Config.FriendlyName,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.End,
                HorizontalTextAlignment = TextAlignment.Start,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var sld = new Slider()
            {
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(20, 0, 20, 0)                
            };

            var thisFader = _faders[i];
            sld.ValueChanged += async (s, e) =>
            {
                if (!_goValueChanges)
                    return;

                _lastSliderUpdate = DateTime.UtcNow;

                if (_faderSetVolume != null)
                    await _faderSetVolume(thisFader.Config, sld.Value);
            };

            var img = new Image()
            {
                Source = "dotnet_bot.png",
                Aspect = Aspect.AspectFit,
                VerticalOptions = LayoutOptions.Center
            };

            var vert = new VerticalStackLayout()
            {
                Children =
                {
                    new Label() { Text="AAA", FontSize=12, LineBreakMode=LineBreakMode.NoWrap, FontAttributes=FontAttributes.Bold },
                    new Label() { Text="BBB", FontSize=10, LineBreakMode=LineBreakMode.NoWrap }
                },
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };

            var scr = new ScrollView()
            {
                Content = vert,
                Margin = new Thickness(4, 2, 4, 2),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            var btn = new Button() { 
                FontSize = 16,
                Text = "\u22ee"
            };

            btn.Clicked += async (s, e) =>
            {
                var exit = new Button() { 
                    Text = "Exit",
                    Margin = new Thickness(3),
                };
                exit.Clicked += async (s, e) =>
                {
                    if (_myPopup != null)
                    {
                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        await _myPopup.CloseAsync(true, cts.Token);
                    }
                };

                _myPopup = new Popup
                {
                    Color = Colors.Transparent,
                    CanBeDismissedByTappingOutsideOfPopup = false,
                    Content = new Border()
                    {
                        BackgroundColor = Colors.Black,
                        StrokeShape = new RoundRectangle() { CornerRadius = new CornerRadius(8) },
                        Content = new VerticalStackLayout
                        {
                            Margin = new Thickness(8),
                            Children =
                            {
                                new Label
                                {
                                    Text = "This is a very important message!",
                                    Margin = new Thickness(2, 10, 2, 10)
                                },
                                new Button
                                {
                                    Text = "Play",
                                    Margin = new Thickness(3),
                                },
                                new Button
                                {
                                    Text = "Pause",
                                    Margin = new Thickness(3),
                                },
                                exit
                            }
                        }
                    }
                };

                var result = await this.ShowPopupAsync(_myPopup, CancellationToken.None);

                if (result is bool boolResult)
                {
                    if (boolResult)
                    {
                        // Yes was tapped
                    }
                    else
                    {
                        // No was tapped
                    }
                }
            };

            Grid.SetRow(lab, 3 * i);     Grid.SetColumn(lab, 0); Grid.SetColumnSpan(lab, 3);
            Grid.SetRow(sld, 3 * i + 1); Grid.SetColumn(sld, 0); Grid.SetColumnSpan(sld, 3);
            Grid.SetRow(img, 3 * i + 2); Grid.SetColumn(img, 0);
            Grid.SetRow(scr, 3 * i + 2); Grid.SetColumn(scr, 1);
            Grid.SetRow(btn, 3 * i + 2); Grid.SetColumn(btn, 2);

            FadersGrid.Children.Add(lab);
            FadersGrid.Children.Add(sld);
            FadersGrid.Children.Add(img);
            FadersGrid.Children.Add(scr);
            FadersGrid.Children.Add(btn);

            _faders[i].Slider = sld;    
            _faders[i].Image = img;    
            _faders[i].Scroll = scr;    
        }

        // at the end, kick off some async options
        await UpdateInfos();

        // start timer
        if (Application.Current != null)
        {
            var timer = Application.Current.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += async (s, e) => await TimerTick(s, e);
            timer.Start();
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (sender == ButtonBack)
        {
            Result = false;
            await Navigation.PopModalAsync();
        }
    }

    private async Task UpdateInfos()
    {
        await Parallel.ForEachAsync(_faders, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, async (fi, ct) =>
        {
            // Important: get fader
            if (_faderGetVolume == null || fi.Slider == null)
                return;

            var vol = await _faderGetVolume(fi.Config);

            if (vol.HasValue)
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Code to run on the main thread
                    _goValueChanges = false;
                    fi.Slider.Value = Math.Max(0.0, Math.Min(100.0, vol.Value));
                    _goValueChanges = true;
                });

            // Less important: get playing info
            if (_faderGetInfo == null || fi.Image == null || fi.Scroll == null)
                return;

            var pi = await _faderGetInfo(fi.Config);
            if (pi == null)
                return;

            // check if already the same display
            if (_playingInfos != null)
            {
                if (_playingInfos.ContainsKey(fi.Config))
                {
                    if (_playingInfos[fi.Config]?.Equals(pi) == true)
                        return;
                    _playingInfos.Remove(fi.Config);
                }
                _playingInfos.Add(fi.Config, pi);
            }

            // made in UI 
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // url?
                if (pi.Url?.HasContent() == true)
                {
                    fi.Image.Source = pi.Url;
                }

                // text?
                if (pi.FirstLine?.HasContent() == true
                    && fi.Scroll.Content is VerticalStackLayout vert
                    && vert.Children.Count >= 2
                    && vert.Children[0] is Label lab1
                    && vert.Children[1] is Label lab2)
                {
                    lab1.Text = (pi.IsPlaying ? "\u27f3 " : " \u26ac ") + pi.FirstLine;
                    lab2.Text = pi.Nextlines;
                }
            });
        });
    }

    private async Task TimerTick(object? sender, EventArgs e)
    {
        await Task.Yield();

        if ((DateTime.UtcNow - _lastSliderUpdate).TotalMilliseconds > 2000)
        {
            await UpdateInfos();
        }
    }
}