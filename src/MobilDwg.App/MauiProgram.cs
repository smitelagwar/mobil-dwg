using Microsoft.Maui.Hosting;

namespace MobilDwg.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        return MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .Build();
    }
}
