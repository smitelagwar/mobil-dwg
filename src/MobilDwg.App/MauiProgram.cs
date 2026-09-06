using Microsoft.Maui.Hosting;
using MobilDwg.App.Viewer;

namespace MobilDwg.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        return MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .UseCadViewport()
            .Build();
    }
}
