namespace heos_maui_app
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += async (s, e) =>
            {
                await MauiUiHelper.ShowToast("AppDomain");
            };
            TaskScheduler.UnobservedTaskException += async (s, e) =>
            {
                await MauiUiHelper.ShowToast("TaskScheduler");
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage());
        }
    }
}