using Android.App;
using Android.Content.PM;
using Android.OS;

namespace heos_maui_app
{
    // MIHO: Changed to Portrait
    // see https://learn.microsoft.com/en-us/answers/questions/990167/lock-screen-orientation-net-maui
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}
