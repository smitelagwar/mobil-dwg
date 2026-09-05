using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui;

namespace MobilDwg.App;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    Exported = true,
    ConfigurationChanges = ConfigChanges.ScreenSize
        | ConfigChanges.Orientation
        | ConfigChanges.UiMode
        | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize
        | ConfigChanges.Density)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataMimeTypes = new[] { "application/acad", "image/vnd.dwg", "image/x-dwg", "application/dxf", "image/vnd.dxf" })]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataSchemes = new[] { "file", "content" },
    DataHost = "*",
    DataPathPatterns = new[] { ".*\\.dwg", ".*\\.dxf" })]
public sealed class MainActivity : MauiAppCompatActivity
{
}
