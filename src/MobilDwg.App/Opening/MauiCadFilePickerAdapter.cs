using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace MobilDwg.App.Opening;

public static class MauiCadFilePickerAdapter
{
    public static async Task<CadFileSelection?> PickAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var picked = await FilePicker.Default.PickAsync(CreateCadPickOptions());
        if (picked is null)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new CadFileSelection(
            picked.FileName,
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
