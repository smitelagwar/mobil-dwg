using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MobilDwg.App;

public sealed class MainPage : ContentPage
{
#if V05_VALIDATION
    private bool _v05Started;
#endif

    public MainPage()
    {
        AutomationId = "v04-real-app-main-page";
        Title = "Mobil DWG";
        BackgroundColor = Color.FromArgb("#0B1220");

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

        Content = layout;
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
}
