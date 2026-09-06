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
    private static string? _pendingCadFile;
    private static Action<string>? _cadFileRequested;
    public static event Action<string>? CadFileRequested
    {
        add
        {
            _cadFileRequested += value;
            if (_pendingCadFile != null && value != null)
            {
                var file = _pendingCadFile;
                _pendingCadFile = null;
                value(file);
            }
        }
        remove
        {
            _cadFileRequested -= value;
        }
    }

    private static void DispatchCadFile(string filePath)
    {
        if (_cadFileRequested != null)
        {
            _cadFileRequested.Invoke(filePath);
        }
        else
        {
            _pendingCadFile = filePath;
        }
    }

    public static event Action? HostPaused;
    public static event Action? HostResumed;
    public static event Action<Android.Content.TrimMemory>? LowMemoryTrimmed;

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        Intent = intent;
        HandleIncomingIntent(intent);
    }

    protected override void OnPause()
    {
        base.OnPause();
        HostPaused?.Invoke();
    }

    protected override void OnResume()
    {
        base.OnResume();
        HostResumed?.Invoke();
        HandleIncomingIntent(Intent);
    }

    private void HandleIncomingIntent(Intent? intent)
    {
        if (intent is null) return;

        var openCad = intent.GetStringExtra("open_cad");
        if (!string.IsNullOrEmpty(openCad))
        {
            intent.RemoveExtra("open_cad");
            DispatchCadFile(openCad);
            return;
        }

        if (intent.Action == Intent.ActionView && intent.Data != null)
        {
            var uri = intent.Data;
            intent.SetAction(null); // Prevent re-trigger on subsequent resume

            Task.Run(async () =>
            {
                try
                {
                    string? targetPath = null;
                    if (string.Equals(uri.Scheme, "file", StringComparison.OrdinalIgnoreCase))
                    {
                        targetPath = uri.Path;
                    }
                    else if (string.Equals(uri.Scheme, "content", StringComparison.OrdinalIgnoreCase))
                    {
                        string displayName = "shared_drawing.dwg";
                        try
                        {
                            using var cursor = ContentResolver?.Query(uri, null, null, null, null);
                            if (cursor != null && cursor.MoveToFirst())
                            {
                                int nameIdx = cursor.GetColumnIndex(Android.Provider.IOpenableColumns.DisplayName);
                                if (nameIdx >= 0)
                                {
                                    var name = cursor.GetString(nameIdx);
                                    if (!string.IsNullOrWhiteSpace(name))
                                    {
                                        displayName = name;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Android.Util.Log.Warn("MobilDwgCAD", $"ACTION_VIEW query display name error: {ex.Message}");
                        }

                        var inputStream = ContentResolver?.OpenInputStream(uri);
                        if (inputStream != null)
                        {
                            var cacheRoot = System.IO.Path.Combine(
                                Microsoft.Maui.Storage.FileSystem.Current.CacheDirectory,
                                "mobil-dwg",
                                "open");
                            var cache = new SafeCadFileCache(cacheRoot);
                            var selection = new CadFileSelection(displayName, -1, _ => ValueTask.FromResult<Stream>(inputStream));
                            var cached = await cache.CopyAsync(selection, 1, null, System.Threading.CancellationToken.None);
                            targetPath = cached.FilePath;
                        }
                    }

                    if (!string.IsNullOrEmpty(targetPath))
                    {
                        Android.Util.Log.Info("MobilDwgCAD", $"ACTION_VIEW resolved targetPath={targetPath}");
                        RunOnUiThread(() => DispatchCadFile(targetPath));
                    }
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Error("MobilDwgCAD", $"ACTION_VIEW_FAIL uri={uri} ex={ex}");
                }
            });
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
        LowMemoryTrimmed?.Invoke(level);

        // B3: Purge orphaned temp files on any trim pressure
        try
        {
            var cacheRoot = System.IO.Path.Combine(
                Microsoft.Maui.Storage.FileSystem.Current.CacheDirectory,
                "mobil-dwg",
                "open");
            var cache = new SafeCadFileCache(cacheRoot);
            cache.PurgeOrphans();
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
