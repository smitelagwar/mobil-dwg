using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using MobilDwg.App.Opening;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;
using MobilDwg.Core.Storage;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;

#if V06_VALIDATION || A10_VALIDATION || A11_VALIDATION || A12_VALIDATION || A13_VALIDATION || A14_VALIDATION || A15_VALIDATION || A16_VALIDATION || A17_VALIDATION || A18_VALIDATION || A19_VALIDATION || A20_VALIDATION || A21_VALIDATION || A22_VALIDATION || A25_VALIDATION || A26_VALIDATION
using Android.Util;
#endif

namespace MobilDwg.App;

public sealed class MainPage : ContentPage
{
    private readonly Button _openButton;
    private readonly Button _cancelButton;
    private readonly Button _closeButton;
    private readonly Label _status;
    private readonly Label _titleLabel;
    private readonly Label _shellReadyLabel;
    private CadFileOpenCoordinator _coordinator;

    private RenderScene? _currentScene;
    private ViewportController? _viewportController;
    private string? _activeDocumentName;
    private string? _activeDocumentVersion;
    private readonly RecentFilesManager _recentManager = new();

    private readonly ScrollView _dashboardView;
    private readonly Grid _viewerView;
    private readonly Image _viewerImage;
    private readonly Label _viewerTitleLabel;
    private readonly Label _viewerVersionBadge;
    private readonly Label _zoomLabel;
    private readonly Label _statsLabel;
    private readonly Button _navCloseButton;
    private readonly Button _navLayerButton;
    private readonly Button _navInfoButton;

    private readonly Grid _layerModalView;
    private readonly VerticalStackLayout _layerStackLayout;
    private readonly Grid _infoModalView;
    private readonly Label _infoContentLabel;

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
#if A21_VALIDATION
    private bool _a21Started;
#endif
#if A22_VALIDATION
    private bool _a22Started;
#endif
#if A25_VALIDATION
    private bool _a25Started;
#endif
#if A26_VALIDATION
    private bool _a26Started;
#endif

    public MainPage()
    {
        AutomationId = "v04-real-app-main-page";
        Title = "Mobil DWG";
        BackgroundColor = Color.FromArgb("#0B0F19");
        _coordinator = CreateCoordinator();

        _titleLabel = new Label
        {
            Text = "Mobil DWG",
            AutomationId = "v04-app-title",
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        };

        _shellReadyLabel = new Label
        {
            Text = "Android app shell ready",
            AutomationId = "v04-shell-ready",
            FontSize = 11,
            TextColor = Color.FromArgb("#0EA5E9"),
            VerticalOptions = LayoutOptions.Center
        };

        _openButton = new Button
        {
            Text = "DWG/DXF seç",
            AutomationId = "v06-open-button",
            BackgroundColor = Color.FromArgb("#2563EB"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            FontSize = 16,
            CornerRadius = 12,
            HeightRequest = 52,
            MinimumHeightRequest = 48
        };
        AutomationProperties.SetName(_openButton, "DWG veya DXF cizim dosyasi sec");
        AutomationProperties.SetHelpText(_openButton, "Sistem dosya secicisini acarak cihazdan guvenli CAD cizim dosyasi secer");
        _openButton.Clicked += OpenClicked;

        _cancelButton = new Button
        {
            Text = "İptal iste",
            AutomationId = "v06-cancel-button",
            BackgroundColor = Color.FromArgb("#1E293B"),
            TextColor = Color.FromArgb("#94A3B8"),
            FontSize = 13,
            CornerRadius = 8,
            HeightRequest = 40,
            MinimumHeightRequest = 40
        };
        AutomationProperties.SetName(_cancelButton, "Cizim acma islemini iptal et");
        AutomationProperties.SetHelpText(_cancelButton, "Devam eden cizim okuma islemini guvenle iptal eder");
        _cancelButton.Clicked += CancelClicked;

        _closeButton = new Button
        {
            Text = "Çizimi kapat",
            AutomationId = "v06-close-button",
            BackgroundColor = Color.FromArgb("#1E293B"),
            TextColor = Color.FromArgb("#EF4444"),
            FontSize = 13,
            CornerRadius = 8,
            HeightRequest = 40,
            MinimumHeightRequest = 40
        };
        AutomationProperties.SetName(_closeButton, "Mevcut cizimi kapat");
        AutomationProperties.SetHelpText(_closeButton, "Acik cizim oturumunu kapatir ve bellegi temizler");
        _closeButton.Clicked += CloseClicked;

        _status = new Label
        {
            Text = "Dosya seçilmedi.",
            AutomationId = "v06-open-status",
            FontSize = 12,
            TextColor = Color.FromArgb("#94A3B8"),
            HorizontalTextAlignment = TextAlignment.Center
        };

        var topHeader = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            Padding = new Thickness(16, 12),
            BackgroundColor = Color.FromArgb("#111827")
        };

        var titleStack = new VerticalStackLayout { Spacing = 2 };
        titleStack.Children.Add(_titleLabel);
        titleStack.Children.Add(_shellReadyLabel);
        topHeader.Add(titleStack, 0, 0);

        var headerActions = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };

        var offlineBadge = new Border
        {
            Stroke = Color.FromArgb("#059669"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            BackgroundColor = Color.FromArgb("#064E3B"),
            Padding = new Thickness(10, 4),
            Content = new Label
            {
                Text = "🔒 %100 Çevrimdışı",
                TextColor = Color.FromArgb("#34D399"),
                FontSize = 11,
                FontAttributes = FontAttributes.Bold
            }
        };
        headerActions.Children.Add(offlineBadge);

        _navLayerButton = new Button
        {
            Text = "📑 Katmanlar",
            BackgroundColor = Color.FromArgb("#1E293B"),
            TextColor = Color.FromArgb("#38BDF8"),
            FontSize = 12,
            CornerRadius = 8,
            HeightRequest = 36,
            IsVisible = false
        };
        _navLayerButton.Clicked += (_, _) => OpenLayerModal();
        headerActions.Children.Add(_navLayerButton);

        _navInfoButton = new Button
        {
            Text = "ℹ️ Bilgi",
            BackgroundColor = Color.FromArgb("#1E293B"),
            TextColor = Color.FromArgb("#A78BFA"),
            FontSize = 12,
            CornerRadius = 8,
            HeightRequest = 36,
            IsVisible = false
        };
        _navInfoButton.Clicked += (_, _) => OpenInfoModal();
        headerActions.Children.Add(_navInfoButton);

        _navCloseButton = new Button
        {
            Text = "✕ Kapat",
            BackgroundColor = Color.FromArgb("#371B1B"),
            TextColor = Color.FromArgb("#F87171"),
            FontSize = 12,
            CornerRadius = 8,
            HeightRequest = 36,
            IsVisible = false
        };
        _navCloseButton.Clicked += (_, _) => CloseActiveDrawing();
        headerActions.Children.Add(_navCloseButton);

        topHeader.Add(headerActions, 1, 0);

        var dashContent = new VerticalStackLayout
        {
            Padding = new Thickness(16, 20),
            Spacing = 20
        };

        var heroCard = new Border
        {
            Stroke = Color.FromArgb("#2563EB"),
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            BackgroundColor = Color.FromArgb("#161F30"),
            Padding = new Thickness(20)
        };
        var heroStack = new VerticalStackLayout { Spacing = 12 };
        heroStack.Children.Add(new Label
        {
            Text = "Cihazınızdaki CAD Çizimlerini Açın",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        });
        heroStack.Children.Add(new Label
        {
            Text = "AutoCAD 2D DWG (R12–2018+) ve DXF dosyalarını internet bağlantısına ihtiyaç duymadan, sıfır veri sızıntısıyla doğrudan cihazınızda yüksek hızda görüntüleyin.",
            FontSize = 13,
            TextColor = Color.FromArgb("#94A3B8"),
            LineHeight = 1.3
        });
        heroStack.Children.Add(_openButton);

        var contractActionsRow = new HorizontalStackLayout
        {
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Center
        };
        contractActionsRow.Children.Add(_cancelButton);
        contractActionsRow.Children.Add(_closeButton);
        heroStack.Children.Add(contractActionsRow);
        heroStack.Children.Add(_status);

        heroCard.Content = heroStack;
        dashContent.Children.Add(heroCard);

        dashContent.Children.Add(new Label
        {
            Text = "HIZLI TEST İÇİN ÖRNEK ÇİZİMLER",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#64748B"),
            Margin = new Thickness(4, 4, 0, 0)
        });

        dashContent.Children.Add(CreateSampleCard(
            "🏢",
            "Apartman 3+1 Kat Planı (Mimari)",
            "Duvarlar, pencereler, kapı yayları, mobilyalar ve ölçülendirmeler",
            "180 Varlık  •  6 Katman",
            async () =>
            {
                var scene = SampleCadDrawings.CreateArchitecturalPlan();
                await DisplayCadSceneAsync(scene, "apartman_kat_plani.dwg", "AutoCAD 2018");
            }));

        dashContent.Children.Add(CreateSampleCard(
            "⚙️",
            "Mekanik Bağlantı Flanşı DN150 (İmalat)",
            "Flanş dairesi, 8 cıvata deliği, merkez eksenleri ve PCD ölçüleri",
            "95 Varlık  •  5 Katman",
            async () =>
            {
                var scene = SampleCadDrawings.CreateMechanicalPart();
                await DisplayCadSceneAsync(scene, "baglanti_flansi_dn150.dwg", "AutoCAD 2018");
            }));

        dashContent.Children.Add(CreateSampleCard(
            "🗺️",
            "Kadastro & İmar Çap Planı (Harita)",
            "Ada ve parsel sınırları, Atatürk Bulvarı ve ED50 poligon noktaları",
            "140 Varlık  •  4 Katman",
            async () =>
            {
                var scene = SampleCadDrawings.CreateSurveyMap();
                await DisplayCadSceneAsync(scene, "kadastro_imar_plani.dxf", "AutoCAD R14");
            }));

        var engineCard = new Border
        {
            Stroke = Color.FromArgb("#1E293B"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            BackgroundColor = Color.FromArgb("#0F172A"),
            Padding = new Thickness(16, 12),
            Margin = new Thickness(0, 10, 0, 20)
        };
        var engineStack = new VerticalStackLayout { Spacing = 4 };
        engineStack.Children.Add(new Label
        {
            Text = "⚙️ Motor ve Performans Bilgisi",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#38BDF8")
        });
        engineStack.Children.Add(new Label
        {
            Text = ".NET 10 MAUI • Skia 2D Çizim Motoru • ACadSharp Okuyucu\nHedef Platform: Android 16 (API 36) • Min: API 24",
            FontSize = 11,
            TextColor = Color.FromArgb("#64748B")
        });
        engineCard.Content = engineStack;
        dashContent.Children.Add(engineCard);

        _dashboardView = new ScrollView { Content = dashContent, IsVisible = true };

        _viewerView = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
            IsVisible = false
        };

        var docBar = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            Padding = new Thickness(16, 8),
            BackgroundColor = Color.FromArgb("#161F30")
        };
        _viewerTitleLabel = new Label
        {
            Text = "Cizim",
            TextColor = Colors.White,
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };
        _viewerVersionBadge = new Label
        {
            Text = "DWG 2018",
            TextColor = Color.FromArgb("#38BDF8"),
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };
        docBar.Add(_viewerTitleLabel, 0, 0);
        docBar.Add(_viewerVersionBadge, 1, 0);
        _viewerView.Add(docBar, 0, 0);

        var canvasGrid = new Grid
        {
            BackgroundColor = Color.FromArgb("#0A0D14")
        };

        _viewerImage = new Image
        {
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        canvasGrid.Children.Add(_viewerImage);

        var panGesture = new PanGestureRecognizer();
        double panAccumX = 0, panAccumY = 0;
        panGesture.PanUpdated += async (_, pe) =>
        {
            if (_viewportController is null) return;
            if (pe.StatusType == GestureStatus.Started)
            {
                panAccumX = 0;
                panAccumY = 0;
            }
            else if (pe.StatusType == GestureStatus.Running)
            {
                double dx = pe.TotalX - panAccumX;
                double dy = pe.TotalY - panAccumY;
                panAccumX = pe.TotalX;
                panAccumY = pe.TotalY;
                _viewportController.Pan(-dx * 2.0, dy * 2.0);
            }
            else if (pe.StatusType == GestureStatus.Completed || pe.StatusType == GestureStatus.Canceled)
            {
                await ReRenderAsync();
            }
        };
        canvasGrid.GestureRecognizers.Add(panGesture);

        var pinchGesture = new PinchGestureRecognizer();
        pinchGesture.PinchUpdated += async (_, pne) =>
        {
            if (_viewportController is null) return;
            if (pne.Status == GestureStatus.Running && pne.Scale > 0.5 && pne.Scale < 2.0)
            {
                _viewportController.PinchZoom(new ScreenPoint2(pne.ScaleOrigin.X * 1080, pne.ScaleOrigin.Y * 1080), pne.Scale);
            }
            else if (pne.Status == GestureStatus.Completed)
            {
                await ReRenderAsync();
            }
        };
        canvasGrid.GestureRecognizers.Add(pinchGesture);

        var doubleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        doubleTap.Tapped += async (_, _) =>
        {
            if (_viewportController is null) return;
            _viewportController.FitExtents();
            await ReRenderAsync();
        };
        canvasGrid.GestureRecognizers.Add(doubleTap);

        _zoomLabel = new Label
        {
            Text = "🔎 Zoom: %100",
            TextColor = Color.FromArgb("#F8FAFC"),
            FontSize = 11,
            FontAttributes = FontAttributes.Bold
        };
        var zoomPill = new Border
        {
            Stroke = Color.FromArgb("#334155"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            BackgroundColor = Color.FromArgb("#CC0F172A"),
            Padding = new Thickness(10, 4),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(14),
            Content = _zoomLabel
        };
        canvasGrid.Children.Add(zoomPill);

        var floatControls = new VerticalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(16)
        };

        var btnZoomIn = CreateFloatingButton("＋", async () =>
        {
            if (_viewportController is null) return;
            _viewportController.ZoomIn(1.25);
            await ReRenderAsync();
        });
        var btnFit = CreateFloatingButton("⤢", async () =>
        {
            if (_viewportController is null) return;
            _viewportController.FitExtents();
            await ReRenderAsync();
        });
        var btnZoomOut = CreateFloatingButton("－", async () =>
        {
            if (_viewportController is null) return;
            _viewportController.ZoomOut(1.25);
            await ReRenderAsync();
        });

        floatControls.Children.Add(btnZoomIn);
        floatControls.Children.Add(btnFit);
        floatControls.Children.Add(btnZoomOut);
        canvasGrid.Children.Add(floatControls);

        _viewerView.Add(canvasGrid, 0, 1);

        var bottomBar = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            Padding = new Thickness(16, 10),
            BackgroundColor = Color.FromArgb("#111827")
        };
        _statsLabel = new Label
        {
            Text = "Hazır",
            TextColor = Color.FromArgb("#94A3B8"),
            FontSize = 12,
            VerticalOptions = LayoutOptions.Center
        };
        var quickLayerBtn = new Button
        {
            Text = "📑 Katmanlar",
            BackgroundColor = Color.FromArgb("#2563EB"),
            TextColor = Colors.White,
            FontSize = 11,
            CornerRadius = 8,
            HeightRequest = 34
        };
        quickLayerBtn.Clicked += (_, _) => OpenLayerModal();

        bottomBar.Add(_statsLabel, 0, 0);
        bottomBar.Add(quickLayerBtn, 1, 0);
        _viewerView.Add(bottomBar, 0, 2);

        _layerModalView = new Grid
        {
            BackgroundColor = Color.FromArgb("#B0000000"),
            IsVisible = false
        };
        var layerCard = new Border
        {
            Stroke = Color.FromArgb("#334155"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            BackgroundColor = Color.FromArgb("#161F30"),
            Padding = new Thickness(20),
            WidthRequest = 340,
            MaximumHeightRequest = 500,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };
        var layerCardStack = new VerticalStackLayout { Spacing = 12 };
        layerCardStack.Children.Add(new Label
        {
            Text = "📑 Çizim Katmanları (Layers)",
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        });
        layerCardStack.Children.Add(new Label
        {
            Text = "Görünürlüğü değiştirmek için katman anahtarlarına dokunun:",
            FontSize = 12,
            TextColor = Color.FromArgb("#94A3B8")
        });

        _layerStackLayout = new VerticalStackLayout { Spacing = 4 };
        var layerScroll = new ScrollView
        {
            Content = _layerStackLayout,
            MaximumHeightRequest = 260
        };
        layerCardStack.Children.Add(layerScroll);

        var layerCloseBtn = new Button
        {
            Text = "Tamam",
            BackgroundColor = Color.FromArgb("#2563EB"),
            TextColor = Colors.White,
            CornerRadius = 10,
            HeightRequest = 44,
            FontAttributes = FontAttributes.Bold
        };
        layerCloseBtn.Clicked += (_, _) => _layerModalView.IsVisible = false;
        layerCardStack.Children.Add(layerCloseBtn);

        layerCard.Content = layerCardStack;
        _layerModalView.Children.Add(layerCard);

        _infoModalView = new Grid
        {
            BackgroundColor = Color.FromArgb("#B0000000"),
            IsVisible = false
        };
        var infoCard = new Border
        {
            Stroke = Color.FromArgb("#334155"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            BackgroundColor = Color.FromArgb("#161F30"),
            Padding = new Thickness(20),
            WidthRequest = 340,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };
        var infoCardStack = new VerticalStackLayout { Spacing = 14 };
        infoCardStack.Children.Add(new Label
        {
            Text = "ℹ️ Çizim Bilgileri ve Teşhis",
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        });
        _infoContentLabel = new Label
        {
            FontSize = 12,
            TextColor = Color.FromArgb("#E2E8F0"),
            LineHeight = 1.4
        };
        infoCardStack.Children.Add(_infoContentLabel);

        var infoCloseBtn = new Button
        {
            Text = "Kapat",
            BackgroundColor = Color.FromArgb("#334155"),
            TextColor = Colors.White,
            CornerRadius = 10,
            HeightRequest = 44
        };
        infoCloseBtn.Clicked += (_, _) => _infoModalView.IsVisible = false;
        infoCardStack.Children.Add(infoCloseBtn);

        infoCard.Content = infoCardStack;
        _infoModalView.Children.Add(infoCard);

        var rootGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };
        rootGrid.Add(topHeader, 0, 0);

        var bodyContainer = new Grid();
        bodyContainer.Children.Add(_dashboardView);
        bodyContainer.Children.Add(_viewerView);
        bodyContainer.Children.Add(_layerModalView);
        bodyContainer.Children.Add(_infoModalView);
        rootGrid.Add(bodyContainer, 0, 1);

#if V05_VALIDATION
        var validationStatus = new Label
        {
            Text = "V05_VALIDATION_PENDING",
            AutomationId = "v05-validation-status",
            FontSize = 12,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        dashContent.Children.Add(validationStatus);
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
        dashContent.Children.Insert(2, a10Status);
        dashContent.Children.Insert(3, a10Image);
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
        dashContent.Children.Insert(2, a11Status);
        dashContent.Children.Insert(3, a11Image);
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
        dashContent.Children.Insert(2, a12Status);
        dashContent.Children.Insert(3, a12Image);
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
        dashContent.Children.Insert(2, a13Status);
        dashContent.Children.Insert(3, a13Image);
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
        dashContent.Children.Insert(2, a14Status);
        dashContent.Children.Insert(3, a14Image);
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
        dashContent.Children.Insert(2, a15Status);
        dashContent.Children.Insert(3, a15Image);
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
        dashContent.Children.Insert(2, a16Status);
        dashContent.Children.Insert(3, a16Image);
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
        dashContent.Children.Insert(2, a17Status);
        dashContent.Children.Insert(3, a17Image);
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
        dashContent.Children.Insert(2, a18Status);
        dashContent.Children.Insert(3, a18Image);
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
        dashContent.Children.Insert(2, a19Status);
        dashContent.Children.Insert(3, a19Image);
        Loaded += async (_, _) => await RunA19ValidationAsync(a19Status, a19Image);
#endif

#if A20_VALIDATION
        var a20Status = new Label
        {
            Text = "A20_VALIDATION_PENDING",
            AutomationId = "a20-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a20Image = new Image
        {
            AutomationId = "a20-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        dashContent.Children.Insert(2, a20Status);
        dashContent.Children.Insert(3, a20Image);
        Loaded += async (_, _) => await RunA20ValidationAsync(a20Status, a20Image);
#endif

#if A21_VALIDATION
        var a21Status = new Label
        {
            Text = "A21_VALIDATION_PENDING",
            AutomationId = "a21-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a21Image = new Image
        {
            AutomationId = "a21-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        dashContent.Children.Insert(2, a21Status);
        dashContent.Children.Insert(3, a21Image);
        Loaded += async (_, _) => await RunA21ValidationAsync(a21Status, a21Image);
#endif

#if A22_VALIDATION
        var a22Status = new Label
        {
            Text = "A22_VALIDATION_PENDING",
            AutomationId = "a22-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a22Image = new Image
        {
            AutomationId = "a22-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        dashContent.Children.Insert(2, a22Status);
        dashContent.Children.Insert(3, a22Image);
        Loaded += async (_, _) => await RunA22ValidationAsync(a22Status, a22Image);
#endif

#if A25_VALIDATION
        var a25Status = new Label
        {
            Text = "A25_VALIDATION_PENDING",
            AutomationId = "a25-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a25Image = new Image
        {
            AutomationId = "a25-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        dashContent.Children.Insert(2, a25Status);
        dashContent.Children.Insert(3, a25Image);
        Loaded += async (_, _) => await RunA25ValidationAsync(a25Status, a25Image);
#endif

#if A26_VALIDATION
        var a26Status = new Label
        {
            Text = "A26_VALIDATION_PENDING",
            AutomationId = "a26-validation-status",
            FontSize = 13,
            TextColor = Color.FromArgb("#B8C4D8"),
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var a26Image = new Image
        {
            AutomationId = "a26-render-image",
            HeightRequest = 420,
            Aspect = Aspect.AspectFit,
            BackgroundColor = Color.FromArgb("#101010"),
        };
        dashContent.Children.Insert(2, a26Status);
        dashContent.Children.Insert(3, a26Image);
        Loaded += async (_, _) => await RunA26ValidationAsync(a26Status, a26Image);
#endif

        Content = rootGrid;
    }

    private static Border CreateSampleCard(string icon, string title, string subtitle, string badge, Action onTap)
    {
        var border = new Border
        {
            Stroke = Color.FromArgb("#1E293B"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            BackgroundColor = Color.FromArgb("#161F30"),
            Padding = new Thickness(14, 12)
        };

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };

        var iconBox = new Border
        {
            Stroke = Color.FromArgb("#334155"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            BackgroundColor = Color.FromArgb("#0F172A"),
            WidthRequest = 42,
            HeightRequest = 42,
            Content = new Label
            {
                Text = icon,
                FontSize = 20,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
        row.Add(iconBox, 0, 0);

        var textStack = new VerticalStackLayout { Spacing = 2 };
        textStack.Children.Add(new Label
        {
            Text = title,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        });
        textStack.Children.Add(new Label
        {
            Text = subtitle,
            FontSize = 11,
            TextColor = Color.FromArgb("#94A3B8"),
            LineBreakMode = LineBreakMode.TailTruncation
        });
        textStack.Children.Add(new Label
        {
            Text = badge,
            FontSize = 10,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#38BDF8"),
            Margin = new Thickness(0, 2, 0, 0)
        });
        row.Add(textStack, 1, 0);

        var openPill = new Border
        {
            Stroke = Color.FromArgb("#2563EB"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            BackgroundColor = Color.FromArgb("#1D4ED8"),
            Padding = new Thickness(10, 6),
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = "Aç ›",
                TextColor = Colors.White,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold
            }
        };
        row.Add(openPill, 2, 0);

        border.Content = row;

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => onTap();
        border.GestureRecognizers.Add(tap);

        return border;
    }

    private static Button CreateFloatingButton(string text, Action onClick)
    {
        var btn = new Button
        {
            Text = text,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#38BDF8"),
            BackgroundColor = Color.FromArgb("#EA0F172A"),
            CornerRadius = 24,
            WidthRequest = 48,
            HeightRequest = 48,
            Padding = 0
        };
        btn.Clicked += (_, _) => onClick();
        return btn;
    }

    private async Task DisplayCadSceneAsync(RenderScene scene, string displayName, string version)
    {
        _currentScene = scene;
        _activeDocumentName = displayName;
        _activeDocumentVersion = version;

        var bounds = scene.WorldBounds ?? new WorldBounds2(0, 0, 100, 100);
        var initialCamera = Camera2D.Fit(bounds, 1080, 1080, paddingFraction: 0.05);
        _viewportController = new ViewportController(initialCamera, bounds);

        _viewerTitleLabel.Text = displayName;
        _viewerVersionBadge.Text = version.Contains("AC", StringComparison.OrdinalIgnoreCase) ? $"DWG {version}" : version;

        await ReRenderAsync();

        _dashboardView.IsVisible = false;
        _viewerView.IsVisible = true;
        _navLayerButton.IsVisible = true;
        _navInfoButton.IsVisible = true;
        _navCloseButton.IsVisible = true;
    }

    private async Task ReRenderAsync()
    {
        if (_currentScene is null || _viewportController is null) return;

        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var camera = _viewportController.CurrentCamera;
            var scene = _currentScene;

            var result = await Task.Run(async () =>
            {
                return await SkiaScenePngRenderer.RenderCameraWithStatsAsync(scene, camera).ConfigureAwait(false);
            });
            sw.Stop();

            var png = result.Png;
            _viewerImage.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));

            var bounds = _viewportController.SceneBounds ?? new WorldBounds2(0, 0, 100, 100);
            var fitCamera = Camera2D.Fit(bounds, camera.PixelWidth, camera.PixelHeight);
            double zoomPercent = Math.Round((fitCamera.WorldUnitsPerPixel / Math.Max(1e-9, camera.WorldUnitsPerPixel)) * 100.0);
            _zoomLabel.Text = $"🔎 Zoom: %{zoomPercent:N0}";

            _statsLabel.Text = $"📦 {scene.Entities.Count} Varlık  •  📑 {scene.LayerTable.Layers.Count} Katman  •  ⚡ {sw.ElapsedMilliseconds} ms";
        }
        catch (Exception ex)
        {
            _status.Text = $"Render: {ex.Message}";
        }
    }

    private void CloseActiveDrawing()
    {
        _currentScene = null;
        _viewportController = null;
        _activeDocumentName = null;
        _activeDocumentVersion = null;

        _viewerView.IsVisible = false;
        _dashboardView.IsVisible = true;
        _navLayerButton.IsVisible = false;
        _navInfoButton.IsVisible = false;
        _navCloseButton.IsVisible = false;
        _status.Text = "Çizim kapatıldı.";
    }

    private void OpenLayerModal()
    {
        if (_currentScene is null) return;

        _layerStackLayout.Children.Clear();
        foreach (var layer in _currentScene.LayerTable.Layers)
        {
            var row = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                Padding = new Thickness(6, 4),
                ColumnSpacing = 12
            };

            var colorBox = new BoxView
            {
                Color = Color.FromUint(layer.Color.Argb),
                WidthRequest = 16,
                HeightRequest = 16,
                CornerRadius = 8,
                VerticalOptions = LayoutOptions.Center
            };
            row.Add(colorBox, 0, 0);

            var nameLabel = new Label
            {
                Text = layer.Name,
                TextColor = Colors.White,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center
            };
            row.Add(nameLabel, 1, 0);

            var toggle = new Switch
            {
                IsToggled = layer.IsVisible,
                OnColor = Color.FromArgb("#2563EB"),
                ThumbColor = Colors.White,
                VerticalOptions = LayoutOptions.Center
            };
            var capturedLayerName = layer.Name;
            toggle.Toggled += async (_, te) =>
            {
                _currentScene.LayerTable.SetLayerVisibility(capturedLayerName, te.Value);
                await ReRenderAsync();
            };
            row.Add(toggle, 2, 0);

            _layerStackLayout.Children.Add(row);
        }

        _layerModalView.IsVisible = true;
    }

    private void OpenInfoModal()
    {
        if (_currentScene is null) return;

        var bounds = _viewportController?.SceneBounds;
        var info = new System.Text.StringBuilder();
        info.AppendLine($"📄 Dosya: {_activeDocumentName ?? "Bilinmiyor"}");
        info.AppendLine($"🔖 Format / Sürüm: {_activeDocumentVersion ?? "CAD"}");
        info.AppendLine($"📦 Toplam Varlık: {_currentScene.Entities.Count:N0}");
        info.AppendLine($"📑 Katman Sayısı: {_currentScene.LayerTable.Layers.Count}");
        if (bounds.HasValue)
        {
            info.AppendLine($"📐 Sınırlar X: [{bounds.Value.MinX:F0} → {bounds.Value.MaxX:F0}]");
            info.AppendLine($"📐 Sınırlar Y: [{bounds.Value.MinY:F0} → {bounds.Value.MaxY:F0}]");
        }
        info.AppendLine($"🔒 Güvenlik: %100 Çevrimdışı (Offline)");
        info.AppendLine($"⚡ Motor: Skia Donanım Hızlandırma");

        _infoContentLabel.Text = info.ToString();
        _infoModalView.IsVisible = true;
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

            if (coordinator.CurrentSession is not null)
            {
                try
                {
                    var extracted = AcadSharpEntityExtractor.Extract(coordinator.CurrentSession.Handle);
                    var scene = CadExtractedSceneBuilder.Build(extracted);
                    await DisplayCadSceneAsync(scene, selection.DisplayName ?? "cizim.dwg", extracted.Version);
                }
                catch (Exception renderEx)
                {
                    _status.Text = $"Çizim yüklendi ({result.Metadata?.Format}), render hazırlığı: {renderEx.Message}";
                }
            }
        }
        catch (CadFileQuotaExceededException)
        {
            _status.Text = "Dosya güvenli kopyalama byte kotasını aşıyor.";
#if V06_VALIDATION
            LogV06("V06_OPEN_FAIL type=CadFileQuotaExceededException");
#endif
#if A25_VALIDATION
            Log.Warn("MobilDwgA25", "A25_RENDER_ERROR_SURFACE_PASS type=CadFileQuotaExceededException");
#endif
            await coordinator.ResetCurrentSessionAsync().ConfigureAwait(false);
        }
        catch (CadFileInsufficientSpaceException)
        {
            _status.Text = "Güvenli özel cache kopyası için yeterli boş alan yok.";
#if V06_VALIDATION
            LogV06("V06_OPEN_FAIL type=CadFileInsufficientSpaceException");
#endif
#if A25_VALIDATION
            Log.Warn("MobilDwgA25", "A25_RENDER_ERROR_SURFACE_PASS type=CadFileInsufficientSpaceException");
#endif
            await coordinator.ResetCurrentSessionAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            var errorKind = exception.GetType().Name;
            _status.Text = $"Dosya açılamadı: {errorKind} — {exception.Message}";
#if V06_VALIDATION
            LogV06($"V06_OPEN_FAIL type={exception.GetType().Name}");
#endif
#if A25_VALIDATION
            Log.Warn("MobilDwgA25", $"A25_RENDER_ERROR_SURFACE_PASS type={errorKind}");
#endif
            await coordinator.ResetCurrentSessionAsync().ConfigureAwait(false);
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
        CloseActiveDrawing();
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
        return System.IO.Path.Combine(FileSystem.Current.CacheDirectory, "mobil-dwg", "open");
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

#if A20_VALIDATION
    private bool _a20Started;

    private async Task RunA20ValidationAsync(Label status, Image image)
    {
        if (_a20Started) return;
        _a20Started = true;
        status.Text = "A20_VALIDATION_RUNNING";
        try
        {
            var result = await Task.Run(A20AndroidValidationRunner.RunAsync);
            var png = result.Png;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
                status.Text = $"{result.Marker} | {result.PerformanceSummary} | {result.MemorySummary}";
            });
            Log.Info(A20AndroidValidationRunner.Tag, $"A20_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                status.Text = $"ANDROID_STAGE20_PERFORMANCE_MEMORY_FAIL: {exception.Message}";
            });
            Log.Error(A20AndroidValidationRunner.Tag, $"ANDROID_STAGE20_PERFORMANCE_MEMORY_FAIL: {exception}");
        }
    }
#endif

#if A21_VALIDATION
    private async Task RunA21ValidationAsync(Label status, Image image)
    {
        if (_a21Started) return;
        _a21Started = true;
        status.Text = "A21_VALIDATION_RUNNING";
        try
        {
            var result = await Task.Run(A21AndroidValidationRunner.RunAsync);
            var png = result.Png;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
                status.Text = $"{result.Marker} | {result.RegressionSummary} | {result.BetaGateSummary}";
            });
            Log.Info(A21AndroidValidationRunner.Tag, $"A21_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                status.Text = $"ANDROID_STAGE21_CORPUS_REGRESSION_FAIL: {exception.Message}";
            });
            Log.Error(A21AndroidValidationRunner.Tag, $"ANDROID_STAGE21_CORPUS_REGRESSION_FAIL: {exception}");
        }
    }
#endif

#if A22_VALIDATION
    private async Task RunA22ValidationAsync(Label status, Image image)
    {
        if (_a22Started) return;
        _a22Started = true;
        status.Text = "A22_VALIDATION_RUNNING";
        try
        {
            var result = await Task.Run(A22AndroidValidationRunner.RunAsync);
            var png = result.Png;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
                status.Text = $"{result.Marker} | {result.PackageSummary} | {result.ComplianceSummary}";
            });
            Log.Info(A22AndroidValidationRunner.Tag, $"A22_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                status.Text = $"ANDROID_STAGE22_RELEASE_RC_FAIL: {exception.Message}";
            });
            Log.Error(A22AndroidValidationRunner.Tag, $"ANDROID_STAGE22_RELEASE_RC_FAIL: {exception}");
        }
    }
#endif

#if V06_VALIDATION
    private static void LogV06(string marker)
    {
        Log.Info("MobilDwgV06", marker);
    }
#endif

#if A25_VALIDATION
    private async Task RunA25ValidationAsync(Label status, Image image)
    {
        if (_a25Started) return;
        _a25Started = true;
        status.Text = "A25_VALIDATION_RUNNING";
        try
        {
            var result = await Task.Run(A25AndroidValidationRunner.RunAsync);
            var png = result.Png;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
                status.Text = $"{result.Marker} | {result.BlockerSummary}";
            });
            Log.Info(A25AndroidValidationRunner.Tag, $"A25_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                status.Text = $"ANDROID_STAGE25_BETA_BLOCKER_FAIL: {exception.Message}";
            });
            Log.Error(A25AndroidValidationRunner.Tag, $"ANDROID_STAGE25_BETA_BLOCKER_FAIL: {exception}");
        }
    }
#endif

#if A26_VALIDATION
    private async Task RunA26ValidationAsync(Label status, Image image)
    {
        if (_a26Started) return;
        _a26Started = true;
        status.Text = "A26_VALIDATION_RUNNING";
        try
        {
            var result = await Task.Run(A26AndroidValidationRunner.RunAsync);
            var png = result.Png;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(png, writable: false));
                status.Text = $"{result.Marker} | {result.AuditSummary}";
            });
            Log.Info(A26AndroidValidationRunner.Tag, $"A26_REAL_APP_UI_IMAGE_READY sha256={result.PngSha256}");
        }
        catch (Exception exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                status.Text = $"ANDROID_STAGE26_RC_APPROVAL_FAIL: {exception.Message}";
            });
            Log.Error(A26AndroidValidationRunner.Tag, $"ANDROID_STAGE26_RC_APPROVAL_FAIL: {exception}");
        }
    }
#endif
}
