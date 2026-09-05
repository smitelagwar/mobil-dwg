using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using MobilDwg.App.Opening;
using MobilDwg.Cad.AcadSharp;

#if V06_VALIDATION || A10_VALIDATION || A11_VALIDATION || A12_VALIDATION || A13_VALIDATION || A14_VALIDATION || A15_VALIDATION || A16_VALIDATION || A17_VALIDATION || A18_VALIDATION || A19_VALIDATION
using Android.Util;
#endif

namespace MobilDwg.App;

public sealed class MainPage : ContentPage
{
    private readonly Button _openButton;
    private readonly Button _cancelButton;
    private readonly Button _closeButton;
    private readonly Label _status;
    private CadFileOpenCoordinator _coordinator;

#if V05_VALIDATION
    private bool _v05Started;
#endif
#if A10_VALIDATION
    private bool _a10Started;
#endif
#if A11_VALIDATION
    private bool _a11Started;
#endif
#if A12_VALIDATION
    private bool _a12Started;
#endif
#if A13_VALIDATION
    private bool _a13Started;
#endif
#if A14_VALIDATION
    private bool _a14Started;
#endif
#if A15_VALIDATION
    private bool _a15Started;
#endif
#if A16_VALIDATION
    private bool _a16Started;
#endif
#if A17_VALIDATION
    private bool _a17Started;
#endif
#if A18_VALIDATION
    private bool _a18Started;
#endif
#if A19_VALIDATION
    private bool _a19Started;
#endif

    public MainPage()
    {
        AutomationId = "v04-real-app-main-page";
        Title = "Mobil DWG";
        BackgroundColor = Color.FromArgb("#0B1220");
        _coordinator = CreateCoordinator();

        _openButton = new Button
        {
            Text = "DWG/DXF seç",
            AutomationId = "v06-open-button",
        };
        _cancelButton = new Button
        {
            Text = "İptal iste",
            AutomationId = "v06-cancel-button",
        };
        _closeButton = new Button
        {
            Text = "Çizimi kapat",
            AutomationId = "v06-close-button",
        };
        _status = new Label
        {
            Text = "Dosya seçilmedi.",
            AutomationId = "v06-open-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };

        _openButton.Clicked += OpenClicked;
        _cancelButton.Clicked += CancelClicked;
        _closeButton.Clicked += CloseClicked;

        var layout = new VerticalStackLayout
        {
            Padding = new Thickness(32),
            Spacing = 14,
            VerticalOptions = LayoutOptions.Center,
        };

        layout.Children.Add(new Label
        {
            Text = "Mobil DWG",
            AutomationId = "v04-app-title",
            FontSize = 30,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalTextAlignment = TextAlignment.Center,
        });
        layout.Children.Add(new Label
        {
            Text = "Android app shell ready",
            AutomationId = "v04-shell-ready",
            FontSize = 16,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        });
        layout.Children.Add(_openButton);
        layout.Children.Add(_cancelButton);
        layout.Children.Add(_closeButton);
        layout.Children.Add(_status);

#if V05_VALIDATION
        var validationStatus = new Label
        {
            Text = "V05_VALIDATION_PENDING",
            AutomationId = "v05-validation-status",
            FontSize = 12,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        layout.Children.Add(validationStatus);
        Loaded += async (_, _) => await RunV05ValidationAsync(validationStatus);
#endif

#if V06_VALIDATION
        Loaded += (_, _) => LogV06("V06_REAL_APP_READY");
#endif

#if A10_VALIDATION
        var a10Status = new Label
        {
            Text = "A10_VALIDATION_PENDING",
            AutomationId = "a10-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a10Image = new Image
        {
            AutomationId = "a10-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        layout.Children.Insert(2, a10Status);
        layout.Children.Insert(3, a10Image);
        Loaded += async (_, _) => await RunA10ValidationAsync(a10Status, a10Image);
#endif

#if A11_VALIDATION
        var a11Status = new Label
        {
            Text = "A11_VALIDATION_PENDING",
            AutomationId = "a11-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a11Image = new Image
        {
            AutomationId = "a11-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        layout.Children.Insert(2, a11Status);
        layout.Children.Insert(3, a11Image);
        Loaded += async (_, _) => await RunA11ValidationAsync(a11Status, a11Image);
#endif

#if A12_VALIDATION
        var a12Status = new Label
        {
            Text = "A12_VALIDATION_PENDING",
            AutomationId = "a12-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a12Image = new Image
        {
            AutomationId = "a12-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        layout.Children.Insert(2, a12Status);
        layout.Children.Insert(3, a12Image);
        Loaded += async (_, _) => await RunA12ValidationAsync(a12Status, a12Image);
#endif

#if A13_VALIDATION
        var a13Status = new Label
        {
            Text = "A13_VALIDATION_PENDING",
            AutomationId = "a13-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a13Image = new Image
        {
            AutomationId = "a13-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        layout.Children.Insert(2, a13Status);
        layout.Children.Insert(3, a13Image);
        Loaded += async (_, _) => await RunA13ValidationAsync(a13Status, a13Image);
#endif

#if A14_VALIDATION
        var a14Status = new Label
        {
            Text = "A14_VALIDATION_PENDING",
            AutomationId = "a14-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a14Image = new Image
        {
            AutomationId = "a14-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        layout.Children.Insert(2, a14Status);
        layout.Children.Insert(3, a14Image);
        Loaded += async (_, _) => await RunA14ValidationAsync(a14Status, a14Image);
#endif

#if A15_VALIDATION
        var a15Status = new Label
        {
            Text = "A15_VALIDATION_PENDING",
            AutomationId = "a15-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a15Image = new Image
        {
            AutomationId = "a15-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        layout.Children.Insert(2, a15Status);
        layout.Children.Insert(3, a15Image);
        Loaded += async (_, _) => await RunA15ValidationAsync(a15Status, a15Image);
#endif

#if A16_VALIDATION
        var a16Status = new Label
        {
            Text = "A16_VALIDATION_PENDING",
            AutomationId = "a16-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a16Image = new Image
        {
            AutomationId = "a16-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        layout.Children.Insert(2, a16Status);
        layout.Children.Insert(3, a16Image);
        Loaded += async (_, _) => await RunA16ValidationAsync(a16Status, a16Image);
#endif

#if A17_VALIDATION
        var a17Status = new Label
        {
            Text = "A17_VALIDATION_PENDING",
            AutomationId = "a17-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a17Image = new Image
        {
            AutomationId = "a17-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        layout.Children.Insert(2, a17Status);
        layout.Children.Insert(3, a17Image);
        Loaded += async (_, _) => await RunA17ValidationAsync(a17Status, a17Image);
#endif

#if A18_VALIDATION
        var a18Status = new Label
        {
            Text = "A18_VALIDATION_PENDING",
            AutomationId = "a18-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a18Image = new Image
        {
            AutomationId = "a18-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        layout.Children.Insert(2, a18Status);
        layout.Children.Insert(3, a18Image);
        Loaded += async (_, _) => await RunA18ValidationAsync(a18Status, a18Image);
#endif

#if A19_VALIDATION
        var a19Status = new Label
        {
            Text = "A19_VALIDATION_PENDING",
            AutomationId = "a19-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a19Image = new Image
        {
            AutomationId = "a19-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        layout.Children.Insert(2, a19Status);
        layout.Children.Insert(3, a19Image);
        Loaded += async (_, _) => await RunA19ValidationAsync(a19Status, a19Image);
#endif

        Content = new ScrollView { Content = layout };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
#if V06_VALIDATION
        LogV06("V06_LIFECYCLE_APPEARING");
#endif
    }

    protected override void OnDisappearing()
    {
        _coordinator.CancelCurrentRequest();
#if V06_VALIDATION
        LogV06("V06_LIFECYCLE_DISAPPEARING");
#endif
        base.OnDisappearing();
    }

    private async void OpenClicked(object? sender, EventArgs e)
    {
        CadFileSelection? selection;
        _openButton.IsEnabled = false;
#if V06_VALIDATION
        LogV06("V06_PICKER_LAUNCH");
#endif
        try
        {
            selection = await MauiCadFilePickerAdapter.PickAsync();
        }
        catch (TaskCanceledException)
        {
            _status.Text = "Dosya seçimi iptal edildi.";
#if V06_VALIDATION
            LogV06("V06_PICKER_CANCEL_PASS");
#endif
            return;
        }
        catch (Exception exception)
        {
            _status.Text = $"Dosya seçilemedi: {exception.GetType().Name}";
#if V06_VALIDATION
            LogV06($"V06_PICKER_FAIL type={exception.GetType().Name}");
#endif
            return;
        }
        finally
        {
            _openButton.IsEnabled = true;
        }

        if (selection is null)
        {
            _status.Text = "Dosya seçimi iptal edildi.";
#if V06_VALIDATION
            LogV06("V06_PICKER_CANCEL_PASS");
#endif
            return;
        }

#if V06_VALIDATION
        LogV06($"V06_PICKER_SELECTION_PASS name={selection.DisplayName ?? "unknown"}");
#endif
        await OpenSelectionAsync(selection);
    }

    private async Task OpenSelectionAsync(CadFileSelection selection)
    {
        var coordinator = _coordinator;
        var progress = new Progress<CadFileOpenProgress>(update =>
        {
            if (!ReferenceEquals(coordinator, _coordinator))
            {
                return;
            }

            if (update.Copy is not null)
            {
                _status.Text = $"Özel cache kopyası: {update.Copy.BytesCopied:N0} byte";
            }
            else if (!string.IsNullOrWhiteSpace(update.Message))
            {
                _status.Text = update.Message;
            }
        });

        try
        {
            var result = await coordinator.OpenLatestAsync(selection, progress);
            if (!ReferenceEquals(coordinator, _coordinator))
            {
                return;
            }

            if (result.Disposition == CadFileOpenDisposition.Superseded)
            {
#if V06_VALIDATION
                LogV06($"V06_SUPERSEDED_PASS generation={result.Generation}");
#endif
                return;
            }

            if (result.Disposition == CadFileOpenDisposition.Cancelled)
            {
                _status.Text = "İptal isteği kaydedildi; geç parser sonucu kullanılmadı.";
#if V06_VALIDATION
                LogV06($"V06_CANCELLED_RESULT_PASS generation={result.Generation}");
#endif
                return;
            }

            _status.Text =
                $"Hazır: {result.Metadata?.Format} {result.Metadata?.AcadVersion ?? "?"}; diagnostics={result.Diagnostics.Count}; compatibility={result.CompatibilityIssues.Count}";
#if V06_VALIDATION
            LogV06(
                $"V06_REAL_APP_SAFE_OPEN_PASS format={result.Metadata?.Format} version={result.Metadata?.AcadVersion ?? "unknown"} generation={result.Generation} diagnostics={result.Diagnostics.Count} compatibility={result.CompatibilityIssues.Count}");
#endif
        }
        catch (CadFileQuotaExceededException)
        {
            _status.Text = "Dosya güvenli kopyalama byte kotasını aşıyor.";
#if V06_VALIDATION
            LogV06("V06_OPEN_FAIL type=CadFileQuotaExceededException");
#endif
        }
        catch (CadFileInsufficientSpaceException)
        {
            _status.Text = "Güvenli özel cache kopyası için yeterli boş alan yok.";
#if V06_VALIDATION
            LogV06("V06_OPEN_FAIL type=CadFileInsufficientSpaceException");
#endif
        }
        catch (Exception exception)
        {
            _status.Text = $"Dosya açılamadı: {exception.GetType().Name}";
#if V06_VALIDATION
            LogV06($"V06_OPEN_FAIL type={exception.GetType().Name}");
#endif
        }
    }

    private void CancelClicked(object? sender, EventArgs e)
    {
        var accepted = _coordinator.CancelCurrentRequest();
        _status.Text = accepted
            ? "İptal istendi. Parser başladıysa geç sonuç UI'a uygulanmayacak."
            : "Aktif açma isteği yok.";
#if V06_VALIDATION
        LogV06($"V06_CANCEL_REQUEST accepted={accepted.ToString().ToLowerInvariant()}");
#endif
    }

    private async void CloseClicked(object? sender, EventArgs e)
    {
        var previous = _coordinator;
        _coordinator = CreateCoordinator();
        await previous.DisposeAsync();

        var remaining = CountCacheFiles();
        _status.Text = "Çizim kapatıldı; session ve app-private cache kopyası temizlendi.";
#if V06_VALIDATION
        if (remaining == 0)
        {
            LogV06("V06_CLOSE_CLEANUP_PASS files=0");
        }
        else
        {
            LogV06($"V06_CLOSE_CLEANUP_FAIL files={remaining}");
        }
#endif
    }

    private static CadFileOpenCoordinator CreateCoordinator()
    {
        return new CadFileOpenCoordinator(
            new AcadSharpDocumentReader(),
            new SafeCadFileCache(GetCacheRoot()));
    }

    private static string GetCacheRoot()
    {
        return Path.Combine(FileSystem.Current.CacheDirectory, "mobil-dwg", "open");
    }

    private static int CountCacheFiles()
    {
        var root = GetCacheRoot();
        return Directory.Exists(root)
            ? Directory.GetFiles(root, "*", SearchOption.AllDirectories).Length
            : 0;
    }

#if V05_VALIDATION
    private async Task RunV05ValidationAsync(Label status)
    {
        if (_v05Started)
        {
            return;
        }

        _v05Started = true;
        status.Text = "V05_VALIDATION_RUNNING";
        try
        {
            var result = await V05AndroidValidationRunner.RunAsync();
            status.Text = result.Marker;
        }
        catch (Exception ex)
        {
            var safeType = ex.GetType().Name;
            status.Text = $"ANDROID_VALIDATION_V05_FAIL type={safeType}";
            Android.Util.Log.Error("MobilDwgV05", status.Text);
        }
    }
#endif

#if A10_VALIDATION
    private async Task RunA10ValidationAsync(Label status, Image image)
    {
        if (_a10Started) return;
        _a10Started = true;
        status.Text = "A10_VALIDATION_RUNNING";
        try
        {
            var result = await A10AndroidValidationRunner.RunAsync();
            var png = result.Png;
            image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
            status.Text = $"{result.Marker} pixels={result.NonBackgroundPixels}";
            Log.Info(A10AndroidValidationRunner.Tag, $"A10_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            status.Text = $"ANDROID_STAGE10_P0_GEOMETRY_RENDER_FAIL type={exception.GetType().Name}";
            Log.Error(A10AndroidValidationRunner.Tag, status.Text);
        }
    }
#endif

#if A11_VALIDATION
    private async Task RunA11ValidationAsync(Label status, Image image)
    {
        if (_a11Started) return;
        _a11Started = true;
        status.Text = "A11_VALIDATION_RUNNING";
        try
        {
            var result = await A11AndroidValidationRunner.RunAsync();
            var png = result.Png;
            image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
            status.Text = $"{result.Marker} pixels={result.NonBackgroundPixels}";
            Log.Info(A11AndroidValidationRunner.Tag, $"A11_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            status.Text = $"ANDROID_STAGE11_VIEWPORT_GESTURE_FAIL type={exception.GetType().Name}";
            Log.Error(A11AndroidValidationRunner.Tag, status.Text);
        }
    }
#endif

#if A12_VALIDATION
    private async Task RunA12ValidationAsync(Label status, Image image)
    {
        if (_a12Started) return;
        _a12Started = true;
        status.Text = "A12_VALIDATION_RUNNING";
        try
        {
            var result = await A12AndroidValidationRunner.RunAsync();
            var png = result.Png;
            image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
            status.Text = $"{result.Marker} entities={result.ExpandedEntityCount}";
            Log.Info(A12AndroidValidationRunner.Tag, $"A12_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            status.Text = $"ANDROID_STAGE12_BLOCK_INSERT_FAIL type={exception.GetType().Name}";
            Log.Error(A12AndroidValidationRunner.Tag, status.Text);
        }
    }
#endif

#if A13_VALIDATION
    private async Task RunA13ValidationAsync(Label status, Image image)
    {
        if (_a13Started) return;
        _a13Started = true;
        status.Text = "A13_VALIDATION_RUNNING";
        try
        {
            var result = await Task.Run(A13AndroidValidationRunner.RunAsync);
            var png = result.Png;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
                status.Text = $"{result.Marker} layers={result.LayerCount}";
            });
            Log.Info(A13AndroidValidationRunner.Tag, $"A13_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                status.Text = $"ANDROID_STAGE13_LAYER_STYLE_FAIL type={exception.GetType().Name}";
            });
            Log.Error(A13AndroidValidationRunner.Tag, $"ANDROID_STAGE13_LAYER_STYLE_FAIL type={exception.GetType().Name}");
        }
    }
#endif

#if A14_VALIDATION
    private async Task RunA14ValidationAsync(Label status, Image image)
    {
        if (_a14Started) return;
        _a14Started = true;
        status.Text = "A14_VALIDATION_RUNNING";
        try
        {
            var result = await Task.Run(A14AndroidValidationRunner.RunAsync);
            var png = result.Png;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
                status.Text = $"{result.Marker} entities={result.TextEntityCount}";
            });
            Log.Info(A14AndroidValidationRunner.Tag, $"A14_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                status.Text = $"ANDROID_STAGE14_TEXT_FONT_FAIL type={exception.GetType().Name}";
            });
            Log.Error(A14AndroidValidationRunner.Tag, $"ANDROID_STAGE14_TEXT_FONT_FAIL type={exception.GetType().Name}");
        }
    }
#endif

#if A15_VALIDATION
    private async Task RunA15ValidationAsync(Label status, Image image)
    {
        if (_a15Started) return;
        _a15Started = true;
        status.Text = "A15_VALIDATION_RUNNING";
        try
        {
            var result = await Task.Run(A15AndroidValidationRunner.RunAsync);
            var png = result.Png;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
                status.Text = $"{result.Marker} entities={result.EntityCount}";
            });
            Log.Info(A15AndroidValidationRunner.Tag, $"A15_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                status.Text = $"ANDROID_STAGE15_DIMENSION_HATCH_FAIL type={exception.GetType().Name}";
            });
            Log.Error(A15AndroidValidationRunner.Tag, $"ANDROID_STAGE15_DIMENSION_HATCH_FAIL type={exception.GetType().Name}");
        }
    }
#endif

#if A16_VALIDATION
    private async Task RunA16ValidationAsync(Label status, Image image)
    {
        if (_a16Started) return;
        _a16Started = true;
        status.Text = "A16_VALIDATION_RUNNING";
        try
        {
            var result = await Task.Run(A16AndroidValidationRunner.RunAsync);
            var png = result.Png;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
                status.Text = $"{result.Marker} layout={result.ActiveLayoutName} entities={result.EntityCount}";
            });
            Log.Info(A16AndroidValidationRunner.Tag, $"A16_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                status.Text = $"ANDROID_STAGE16_LAYOUT_VIEWPORT_FAIL: {exception.Message}";
            });
            Log.Error(A16AndroidValidationRunner.Tag, $"ANDROID_STAGE16_LAYOUT_VIEWPORT_FAIL: {exception}");
        }
    }
#endif

#if A17_VALIDATION
    private async Task RunA17ValidationAsync(Label status, Image image)
    {
        if (_a17Started) return;
        _a17Started = true;
        status.Text = "A17_VALIDATION_RUNNING";
        try
        {
            var result = await Task.Run(A17AndroidValidationRunner.RunAsync);
            var png = result.Png;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
                status.Text = $"{result.Marker} entities={result.EntityCount}";
            });
            Log.Info(A17AndroidValidationRunner.Tag, $"A17_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                status.Text = $"ANDROID_STAGE17_XREF_COMPAT_FAIL: {exception.Message}";
            });
            Log.Error(A17AndroidValidationRunner.Tag, $"ANDROID_STAGE17_XREF_COMPAT_FAIL: {exception}");
        }
    }
#endif

#if A18_VALIDATION
    private async Task RunA18ValidationAsync(Label status, Image image)
    {
        if (_a18Started) return;
        _a18Started = true;
        status.Text = "A18_VALIDATION_RUNNING";
        try
        {
            var result = await Task.Run(A18AndroidValidationRunner.RunAsync);
            var png = result.Png;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
                status.Text = $"{result.Marker} doc={result.DocumentName} layout={result.ActiveLayoutName} recent={result.RecentCount}";
            });
            Log.Info(A18AndroidValidationRunner.Tag, $"A18_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                status.Text = $"ANDROID_STAGE18_VIEWER_LIFECYCLE_FAIL: {exception.Message}";
            });
            Log.Error(A18AndroidValidationRunner.Tag, $"ANDROID_STAGE18_VIEWER_LIFECYCLE_FAIL: {exception}");
        }
    }
#endif

#if A19_VALIDATION
    private async Task RunA19ValidationAsync(Label status, Image image)
    {
        if (_a19Started) return;
        _a19Started = true;
        status.Text = "A19_VALIDATION_RUNNING";
        try
        {
            var result = await Task.Run(A19AndroidValidationRunner.RunAsync);
            var png = result.Png;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
                status.Text = $"{result.Marker} {result.PreflightSummary}";
            });
            Log.Info(A19AndroidValidationRunner.Tag, $"A19_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                status.Text = $"ANDROID_STAGE19_RESOURCE_GUARDS_FAIL: {exception.Message}";
            });
            Log.Error(A19AndroidValidationRunner.Tag, $"ANDROID_STAGE19_RESOURCE_GUARDS_FAIL: {exception}");
        }
    }
#endif

#if V06_VALIDATION
    private static void LogV06(string marker)
    {
        Log.Info("MobilDwgV06", marker);
    }
#endif
}
