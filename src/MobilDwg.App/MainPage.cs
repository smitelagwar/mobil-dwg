using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MobilDwg.App;

public sealed class MainPage : ContentPage
{
    public MainPage()
    {
        AutomationId = "v04-real-app-main-page";
        Title = "Mobil DWG";
        BackgroundColor = Color.FromArgb("#0B1220");

        Content = new VerticalStackLayout
        {
            Padding = new Thickness(32),
            Spacing = 14,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = "Mobil DWG",
                    AutomationId = "v04-app-title",
                    FontSize = 30,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                },
                new Label
                {
                    Text = "Android app shell ready",
                    AutomationId = "v04-shell-ready",
                    FontSize = 16,
                    TextColor = Color.FromArgb("#B8C4D8"),
                    HorizontalTextAlignment = TextAlignment.Center,
                },
            },
        };
    }
}
