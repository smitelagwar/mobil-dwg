using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui;
using MobilDwg.App.Opening;

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
    DataPathPatterns = new[] { @".*\.dwg", @".*\.dxf" })]
public sealed class MainActivity : MauiAppCompatActivity
{
    public static event Action<string>? CadFileRequested;

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        Intent = intent;
        var openCad = intent?.GetStringExtra("open_cad");
        if (!string.IsNullOrEmpty(openCad))
        {
            CadFileRequested?.Invoke(openCad);
        }
    }

    /// <summary>
    /// Called by Android OS when memory is low. Purges any orphaned temporary CAD cache
    /// files left in the app-private cache directory. Normal operation: each CachedCadFile
    /// disposes itself; this is a belt-and-suspenders sweep for crash-interrupted sessions.
    /// </summary>
    public override void OnTrimMemory(Android.Content.TrimMemory level)
    {
        base.OnTrimMemory(level);

        // B3: Purge orphaned temp files on any trim pressure
        try
        {
            var cacheRoot = System.IO.Path.Combine(
                Microsoft.Maui.Storage.FileSystem.Current.CacheDirectory,
                "mobil-dwg",
                "open");
            var cache = new SafeCadFileCache(cacheRoot);
            cache.PurgeAll();
#if A25_VALIDATION
            Android.Util.Log.Info("MobilDwgA25", $"A25_CACHE_PURGE_PASS level={level}");
#endif
        }
        catch (Exception ex)
        {
            Android.Util.Log.Warn("MobilDwgA25", $"A25_CACHE_PURGE_WARN: {ex.GetType().Name}");
        }
    }
}
