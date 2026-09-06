using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MobilDwg.App.Viewer;

public static class ViewerHostingExtensions
{
    public static MauiAppBuilder UseCadViewport(this MauiAppBuilder builder)
    {
        return builder.UseSkiaSharp();
    }
}
