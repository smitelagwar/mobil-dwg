using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using MobilDwg.App.Opening;
using MobilDwg.Cad.AcadSharp;

#if V06_VALIDATION
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

#if V06_VALIDATION
    private static void LogV06(string marker)
    {
        Log.Info("MobilDwgV06", marker);
    }
#endif
}
