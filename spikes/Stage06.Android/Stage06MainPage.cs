using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using MobilDwg.App.Opening;
using MobilDwg.Cad.AcadSharp;

namespace Stage06AndroidSmoke;

public sealed class Stage06MainPage : ContentPage
{
    private readonly Button _openButton;
    private readonly Button _cancelButton;
    private readonly Button _closeButton;
    private readonly Label _status;
    private CadFileOpenCoordinator _coordinator;

    public Stage06MainPage()
    {
        Title = "Mobil DWG/DXF Stage 06";
        _coordinator = CreateCoordinator();

        _openButton = new Button { Text = "DWG/DXF seç" };
        _cancelButton = new Button { Text = "İptal iste" };
        _closeButton = new Button { Text = "Çizimi kapat" };
        _status = new Label { Text = "Dosya seçilmedi." };

        _openButton.Clicked += OpenClicked;
        _cancelButton.Clicked += CancelClicked;
        _closeButton.Clicked += CloseClicked;

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 12,
                Children =
                {
                    _openButton,
                    _cancelButton,
                    _closeButton,
                    _status,
                },
            },
        };
    }

    protected override void OnDisappearing()
    {
        _coordinator.CancelCurrentRequest();
        base.OnDisappearing();
    }

    private async void OpenClicked(object? sender, EventArgs e)
    {
        _openButton.IsEnabled = false;
        try
        {
            var picked = await FilePicker.Default.PickAsync(CreateCadPickOptions());
            if (picked is null)
            {
                _status.Text = "Dosya seçimi iptal edildi.";
                return;
            }

            var selection = new CadFileSelection(
                picked.FileName,
                declaredLength: null,
                cancellationToken => OpenPickedStreamAsync(picked, cancellationToken));

            var progress = new Progress<CadFileOpenProgress>(update =>
            {
                if (update.Copy is not null)
                {
                    _status.Text = $"Özel cache kopyası: {update.Copy.BytesCopied:N0} byte";
                }
                else if (!string.IsNullOrWhiteSpace(update.Message))
                {
                    _status.Text = update.Message;
                }
            });

            var result = await _coordinator.OpenLatestAsync(selection, progress);
            _status.Text = result.Disposition switch
            {
                CadFileOpenDisposition.Ready =>
                    $"Hazır: {result.Metadata?.Format} {result.Metadata?.AcadVersion ?? "?"}; diagnostics={result.Diagnostics.Count}; compatibility={result.CompatibilityIssues.Count}",
                CadFileOpenDisposition.Cancelled =>
                    "İptal isteği kaydedildi. Parser başladıysa arka planda tamamlanmış olabilir; sonucu kullanılmadı.",
                CadFileOpenDisposition.Superseded =>
                    "Bu sonuç daha yeni bir dosya seçimi tarafından geçersiz kılındı.",
                _ => "Bilinmeyen sonuç.",
            };
        }
        catch (CadFileQuotaExceededException)
        {
            _status.Text = "Dosya güvenli kopyalama byte kotasını aşıyor.";
        }
        catch (CadFileInsufficientSpaceException)
        {
            _status.Text = "Güvenli özel cache kopyası için yeterli boş alan yok.";
        }
        catch (TaskCanceledException)
        {
            _status.Text = "Dosya seçimi iptal edildi.";
        }
        catch (Exception exception)
        {
            _status.Text = $"Dosya açılamadı: {exception.GetType().Name}";
        }
        finally
        {
            _openButton.IsEnabled = true;
        }
    }

    private void CancelClicked(object? sender, EventArgs e)
    {
        _status.Text = _coordinator.CancelCurrentRequest()
            ? "İptal istendi. ACadSharp parse başladıysa hard-stop sözü verilmez; geç sonuç UI'a uygulanmayacak."
            : "Aktif açma isteği yok.";
    }

    private async void CloseClicked(object? sender, EventArgs e)
    {
        await _coordinator.DisposeAsync();
        _coordinator = CreateCoordinator();
        _status.Text = "Çizim kapatıldı; session ve app-private cache kopyası temizlendi.";
    }

    private static CadFileOpenCoordinator CreateCoordinator()
    {
        var cacheRoot = Path.Combine(FileSystem.Current.CacheDirectory, "mobil-dwg", "stage06-open");
        return new CadFileOpenCoordinator(
            new AcadSharpDocumentReader(),
            new SafeCadFileCache(cacheRoot));
    }

    private static PickOptions CreateCadPickOptions()
    {
        var types = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.Android] =
            [
                "application/acad",
                "application/x-acad",
                "application/dwg",
                "image/vnd.dwg",
                "application/dxf",
                "application/x-dxf",
                "application/octet-stream",
                "text/plain",
            ],
        });

        return new PickOptions
        {
            PickerTitle = "DWG veya DXF seç",
            FileTypes = types,
        };
    }

    private static async ValueTask<Stream> OpenPickedStreamAsync(
        FileResult picked,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var stream = await picked.OpenReadAsync().ConfigureAwait(false);
        if (cancellationToken.IsCancellationRequested)
        {
            await stream.DisposeAsync().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }

        return stream;
    }
}
