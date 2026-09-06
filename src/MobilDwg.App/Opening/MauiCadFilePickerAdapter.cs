using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace MobilDwg.App.Opening;

public static class MauiCadFilePickerAdapter
{
    public static async Task<CadFileSelection?> PickAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        FileResult? picked = null;
        try
        {
            var multiple = await FilePicker.Default.PickMultipleAsync(CreateCadPickOptions());
            if (multiple is not null)
            {
                picked = multiple.FirstOrDefault();
            }
        }
        catch
        {
            // Fallback to single pick if multi-pick fails on specific platform variant
            picked = await FilePicker.Default.PickAsync(CreateCadPickOptions());
        }

        if (picked is null)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var fileName = picked.FileName;
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            try
            {
                fileName = System.Net.WebUtility.UrlDecode(fileName);
            }
            catch
            {
            }
        }

        return new CadFileSelection(
            fileName,
            declaredLength: null,
            token => OpenPickedStreamAsync(picked, token));
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
                "application/x-dwg",
                "image/vnd.dwg",
                "image/x-dwg",
                "application/dxf",
                "application/x-dxf",
                "image/vnd.dxf",
                "image/x-dxf",
                "application/octet-stream",
                "text/plain",
                "*/*",
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
