using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using MobilDwg.App.Opening;
using MobilDwg.App.Viewer;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;
using MobilDwg.Core.Storage;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Layouts;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Skia;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Viewer;

#if V06_VALIDATION || A10_VALIDATION || A11_VALIDATION || A12_VALIDATION || A13_VALIDATION || A14_VALIDATION || A15_VALIDATION || A16_VALIDATION || A17_VALIDATION || A18_VALIDATION || A19_VALIDATION || A20_VALIDATION || A21_VALIDATION || A22_VALIDATION || A25_VALIDATION || A26_VALIDATION
using Android.Util;
#endif

namespace MobilDwg.App;

public sealed class MainPage : ContentPage
{
    // Modern Dark CAD Tasarım Renk Token'ları
    private static readonly Color ColorBgCanvas = Color.FromArgb("#080B11");
    private static readonly Color ColorBgSurface = Color.FromArgb("#0F172A");
    private static readonly Color ColorBgSurfaceElevated = Color.FromArgb("#162032");
    private static readonly Color ColorBorderSubtle = Color.FromArgb("#1E293B");
    private static readonly Color ColorBorderHighlight = Color.FromArgb("#38BDF8");
    private static readonly Color ColorAccentBlue = Color.FromArgb("#2563EB");
    private static readonly Color ColorAccentCyan = Color.FromArgb("#0EA5E9");
    private static readonly Color ColorAccentEmerald = Color.FromArgb("#10B981");
    private static readonly Color ColorAccentAmber = Color.FromArgb("#F59E0B");
    private static readonly Color ColorAccentRose = Color.FromArgb("#EF4444");
    private static readonly Color ColorTextPrimary = Color.FromArgb("#F8FAFC");
    private static readonly Color ColorTextSecondary = Color.FromArgb("#94A3B8");
    private static readonly Color ColorTextMuted = Color.FromArgb("#64748B");

    private readonly Button _openButton;
    private readonly Button _cancelButton;
    private readonly Button _closeButton;
    private readonly Label _status;
    private readonly Label _titleLabel;
    private readonly Label _shellReadyLabel;
    private CadFileOpenCoordinator _coordinator;

    private RenderScene? _currentScene;
    private CadViewerSession? _session;
    public ViewportController? ViewportController => _session?.Controller;
    private ViewportController? _viewportController => _session?.Controller;
    private string? _activeDocumentName;
    private string? _activeDocumentVersion;
    private readonly RecentFilesManager _recentManager = new();

    private readonly ScrollView _dashboardView;
    private readonly Grid _viewerView;
    private readonly Grid _canvasGrid;
    private readonly CadViewportView _viewportView;
    private readonly Border _transitionOverlay;
    private readonly Label _viewerTitleLabel;
    private readonly Label _viewerVersionBadge;
    private readonly Label _zoomLabel;
    private readonly Label _latencyLabel;
    private readonly Label _statsLabel;
    private readonly Button _navCloseButton;
    private readonly Button _navLayerButton;
    private readonly Button _navInfoButton;

    private bool _isLightMode;
    private bool _isMeasureMode;
    private WorldPoint2? _measureStartPoint;
    private readonly Border _measureHud;
    private readonly Label _measureLabel;
    private readonly Border _floatingIslandBar;
    private readonly Button _islandThemeButton;
    private readonly Button _islandMeasureButton;
    private readonly Button _islandFitButton;

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
        BackgroundColor = ColorBgCanvas;
        _coordinator = CreateCoordinator();

#if ANDROID
        MainActivity.CadFileRequested += fileName =>
        {
            Dispatcher.Dispatch(async () =>
            {
                await OpenDesktopCadFileAsync(fileName, fileName);
            });
        };
#endif

        _titleLabel = new Label
        {
            Text = "Mobil DWG",
            AutomationId = "v04-app-title",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = ColorTextPrimary,
            VerticalOptions = LayoutOptions.Center
        };

        _shellReadyLabel = new Label
        {
            Text = "Android app shell ready",
            AutomationId = "v04-shell-ready",
            FontSize = 10,
            TextColor = ColorBorderHighlight,
            VerticalOptions = LayoutOptions.Center
        };

        _openButton = new Button
        {
            Text = "📂 DWG / DXF Seç ›",
            AutomationId = "v06-open-button",
            BackgroundColor = ColorAccentBlue,
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            FontSize = 16,
            CornerRadius = 14,
            HeightRequest = 54,
            MinimumHeightRequest = 48,
            Shadow = new Shadow { Brush = ColorAccentBlue, Opacity = 0.35f, Radius = 8, Offset = new Point(0, 3) }
        };
        AutomationProperties.SetName(_openButton, "DWG veya DXF cizim dosyasi sec");
        AutomationProperties.SetHelpText(_openButton, "Sistem dosya secicisini acarak cihazdan guvenli CAD cizim dosyasi secer");
        _openButton.Clicked += OpenClicked;

        _cancelButton = new Button
        {
            Text = "İptal iste",
            AutomationId = "v06-cancel-button",
            BackgroundColor = Color.FromArgb("#1E293B"),
            TextColor = ColorTextSecondary,
            FontSize = 13,
            CornerRadius = 10,
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
            TextColor = ColorAccentRose,
            FontSize = 13,
            CornerRadius = 10,
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
            TextColor = ColorTextSecondary,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var topHeader = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            Padding = new Thickness(18, 12),
            BackgroundColor = Color.FromArgb("#0D131F")
        };

        var logoBox = new Border
        {
            Stroke = ColorBorderHighlight,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            BackgroundColor = Color.FromArgb("#1E293B"),
            WidthRequest = 38,
            HeightRequest = 38,
            Content = new Label
            {
                Text = "⚡",
                FontSize = 20,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };

        var titleStack = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        titleStack.Children.Add(_titleLabel);
        titleStack.Children.Add(_shellReadyLabel);

        var titleRow = new HorizontalStackLayout { Spacing = 10, VerticalOptions = LayoutOptions.Center };
        titleRow.Children.Add(logoBox);
        titleRow.Children.Add(titleStack);
        topHeader.Add(titleRow, 0, 0);

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
        topHeader.Add(headerActions, 1, 0);

        var dashContent = new VerticalStackLayout
        {
            Padding = new Thickness(16, 16),
            Spacing = 18
        };

        var heroCard = new Border
        {
            Stroke = ColorAccentBlue,
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            BackgroundColor = ColorBgSurface,
            Padding = new Thickness(20),
            Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.4f, Radius = 12, Offset = new Point(0, 4) }
        };
        var heroStack = new VerticalStackLayout { Spacing = 12 };

        var heroPill = new Border
        {
            Stroke = Color.FromArgb("#1E3A8A"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            BackgroundColor = Color.FromArgb("#1E293B"),
            Padding = new Thickness(8, 3),
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = "✨ YEREL CAD GÖRÜNTÜLEYİCİ",
                TextColor = ColorBorderHighlight,
                FontSize = 10,
                FontAttributes = FontAttributes.Bold
            }
        };
        heroStack.Children.Add(heroPill);

        heroStack.Children.Add(new Label
        {
            Text = "Cihazınızdaki CAD Çizimlerini Açın",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = ColorTextPrimary
        });
        heroStack.Children.Add(new Label
        {
            Text = "AutoCAD 2D DWG (R12–2018+) ve DXF dosyalarını internet bağlantısına ihtiyaç duymadan, sıfır veri sızıntısıyla doğrudan cihazınızda yüksek hızda görüntüleyin.",
            FontSize = 12,
            TextColor = ColorTextSecondary,
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
            Text = "🏢 MASAÜSTÜ GERÇEK CAD PROJELERİ",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = ColorBorderHighlight,
            Margin = new Thickness(4, 4, 0, 0)
        });

        dashContent.Children.Add(CreateSampleCard(
            "📐",
            "1 ve 2.kat dwg.dwg",
            "Kat mimari ve kalıp planı • 6.869 Varlık • 40 Katman",
            "3.28 MB • DWG 2018",
            async () => await OpenDesktopCadFileAsync("1 ve 2.kat dwg.dwg", "1 ve 2.kat dwg.dwg", "1_ve_2_kat_dwg.dwg"),
            "desktop-card-dwg1"));

        dashContent.Children.Add(CreateSampleCard(
            "🏗️",
            "SÜHEYLA KARA STATİK (HAFİF).dwg",
            "Statik betonarme kalıp & donatı projesi • 131.655 Varlık • 68 Katman",
            "7.32 MB • DWG 2018",
            async () => await OpenDesktopCadFileAsync("SÜHEYLA KARA STATİK (HAFİF) - Kopya.dwg", "SÜHEYLA KARA STATİK (HAFİF) - Kopya.dwg", "suheyla_kara_statik.dwg"),
            "desktop-card-dwg2"));

        dashContent.Children.Add(CreateSampleCard(
            "⚡",
            "SÜHEYLA KARA STATİK (HAFİF).dxf",
            "Vektör CAD veri değişimi formatı • 131.655 Varlık • 69 Katman",
            "60.52 MB • DXF 2004",
            async () => await OpenDesktopCadFileAsync("SÜHEYLA KARA STATİK (HAFİF) - Kopya.dxf", "SÜHEYLA KARA STATİK (HAFİF) - Kopya.dxf", "suheyla_kara_statik.dxf"),
            "desktop-card-dxf3"));

        dashContent.Children.Add(new Label
        {
            Text = "📐 HIZLI TEST İÇİN ÖRNEK ÇİZİMLER",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = ColorAccentEmerald,
            Margin = new Thickness(4, 8, 0, 0)
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
            Stroke = ColorBorderSubtle,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            BackgroundColor = Color.FromArgb("#0D131F"),
            Padding = new Thickness(16, 12),
            Margin = new Thickness(0, 8, 0, 20)
        };
        var engineStack = new VerticalStackLayout { Spacing = 4 };
        engineStack.Children.Add(new Label
        {
            Text = "⚙️ Motor ve Performans Bilgisi",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = ColorBorderHighlight
        });
        engineStack.Children.Add(new Label
        {
            Text = ".NET 10 MAUI • Skia 2D Çizim Motoru • CAD Okuyucu\nHedef Platform: Android 16 (API 36) • Min: API 24",
            FontSize = 11,
            TextColor = ColorTextMuted
        });
        engineCard.Content = engineStack;
        dashContent.Children.Add(engineCard);

        _dashboardView = new ScrollView { Content = dashContent, IsVisible = true };

        // --- VIEWER ÇALIŞMA ALANI (CAD WORKSPACE) ---
        _viewerView = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star)
            },
            IsVisible = false
        };

        _canvasGrid = new Grid
        {
            BackgroundColor = Color.FromArgb("#0A0D14")
        };

        _transitionOverlay = new Border
        {
            BackgroundColor = ColorBgCanvas,
            IsVisible = false,
            InputTransparent = true,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        _viewportView = new CadViewportView
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        _viewportView.FramePresented += _ =>
        {
            Dispatcher.Dispatch(() =>
            {
                if (_transitionOverlay?.IsVisible == true)
                {
                    _transitionOverlay.IsVisible = false;
                }
            });
        };
        _canvasGrid.Children.Add(_viewportView);
        _canvasGrid.Children.Add(_transitionOverlay);

        // Dynamically resize camera to exact screen canvas dimensions on layout / orientation changes
        _canvasGrid.SizeChanged += (_, _) =>
        {
            if (_session is null) return;
            var (pw, ph) = GetViewportPixelDimensions();
            if (pw > 50 && ph > 50 && (_session.ViewportPixelWidth != pw || _session.ViewportPixelHeight != ph))
            {
                _session.ResizeViewport(pw, ph);
                _viewportView.RequestFrame();
                UpdateHud();
            }
        };

        // Top Floating HUD Bar (Document Title, Version, Latency, Zoom)
        _viewerTitleLabel = new Label
        {
            Text = "Çizim",
            TextColor = ColorTextPrimary,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        };

        _viewerVersionBadge = new Label
        {
            Text = "DWG 2018",
            TextColor = ColorBorderHighlight,
            FontSize = 10,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };

        _zoomLabel = new Label
        {
            Text = "🔎 %100",
            TextColor = ColorTextPrimary,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };

        _latencyLabel = new Label
        {
            Text = "⚡ 0 ms",
            TextColor = ColorAccentEmerald,
            FontSize = 10,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };

        _statsLabel = new Label
        {
            Text = "Hazır",
            TextColor = ColorTextSecondary,
            FontSize = 11,
            VerticalOptions = LayoutOptions.Center
        };

        _navCloseButton = new Button
        {
            Text = "✕",
            BackgroundColor = Color.FromArgb("#371B1B"),
            TextColor = Color.FromArgb("#F87171"),
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 10,
            WidthRequest = 34,
            HeightRequest = 34,
            Padding = 0,
            VerticalOptions = LayoutOptions.Center
        };
        _navCloseButton.Clicked += (_, _) => CloseActiveDrawing();

        var docBar = new Border
        {
            Stroke = ColorBorderSubtle,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            BackgroundColor = Color.FromArgb("#E60D131F"),
            Padding = new Thickness(12, 8),
            Margin = new Thickness(12, 10),
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Start,
            Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.4f, Radius = 10, Offset = new Point(0, 3) }
        };

        var docGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10
        };
        docGrid.Add(_navCloseButton, 0, 0);

        var docTitleStack = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        docTitleStack.Children.Add(_viewerTitleLabel);
        docTitleStack.Children.Add(_viewerVersionBadge);
        docGrid.Add(docTitleStack, 1, 0);

        var hudBadges = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        hudBadges.Children.Add(_latencyLabel);
        hudBadges.Children.Add(_zoomLabel);
        docGrid.Add(hudBadges, 2, 0);

        docBar.Content = docGrid;
        _canvasGrid.Children.Add(docBar);

        // Floating Measure HUD Indicator
        _measureLabel = new Label
        {
            Text = "📏 Ölçüm Modu: 1. Noktaya dokunun...",
            TextColor = Color.FromArgb("#FCD34D"),
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };
        _measureHud = new Border
        {
            Stroke = ColorAccentAmber,
            StrokeThickness = 1.2,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            BackgroundColor = Color.FromArgb("#F0182234"),
            Padding = new Thickness(14, 8),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 68, 0, 0),
            IsVisible = false,
            Content = _measureLabel,
            Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.5f, Radius = 12, Offset = new Point(0, 4) }
        };
        _canvasGrid.Children.Add(_measureHud);

        // Right Floating Zoom Controls (FABs)
        var floatControls = new Border
        {
            Stroke = Color.FromArgb("#334155"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            BackgroundColor = Color.FromArgb("#EE0B101D"),
            Padding = new Thickness(4, 6),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 12, 0),
            Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.4f, Radius = 12, Offset = new Point(0, 4) }
        };

        var fabStack = new VerticalStackLayout { Spacing = 4 };
        var btnZoomIn = CreateFloatingButton("＋", () =>
        {
            if (_session is null) return;
            _session.Controller.ZoomIn(1.35);
            _viewportView.RequestFrame();
            UpdateHud();
        });
        var btnFit = CreateFloatingButton("⤢", () =>
        {
            if (_session is null) return;
            _session.ZoomToFit();
            UpdateHud();
        });
        var btnZoomOut = CreateFloatingButton("－", () =>
        {
            if (_session is null) return;
            _session.Controller.ZoomOut(1.35);
            _viewportView.RequestFrame();
            UpdateHud();
        });
        fabStack.Children.Add(btnZoomIn);
        fabStack.Children.Add(btnFit);
        fabStack.Children.Add(btnZoomOut);
        floatControls.Content = fabStack;
        _canvasGrid.Children.Add(floatControls);

        // Floating Bottom Island Toolbar (Thumb Zone Ergonomics)
        _floatingIslandBar = new Border
        {
            Stroke = Color.FromArgb("#334155"),
            StrokeThickness = 1.2,
            StrokeShape = new RoundRectangle { CornerRadius = 26 },
            BackgroundColor = Color.FromArgb("#F20B101D"),
            Padding = new Thickness(8, 6),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(16, 0, 16, 18),
            Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.6f, Radius = 18, Offset = new Point(0, 6) }
        };

        var islandStack = new HorizontalStackLayout { Spacing = 6 };
        _navLayerButton = CreateIslandButton("📑 Katmanlar", ColorBorderHighlight, () => OpenLayerModal());
        _islandMeasureButton = CreateIslandButton("📏 Ölçü", ColorAccentAmber, () => ToggleMeasureMode());
        _islandThemeButton = CreateIslandButton("☀️ Tema", Color.FromArgb("#E2E8F0"), async () => await ToggleThemeModeAsync());
        _islandFitButton = CreateIslandButton("⤢ Sığdır", ColorBorderHighlight, () =>
        {
            if (_session is null) return;
            _session.ZoomToFit();
            UpdateHud();
        });
        _navInfoButton = CreateIslandButton("ℹ️ Bilgi", Color.FromArgb("#A78BFA"), () => OpenInfoModal());

        islandStack.Children.Add(_navLayerButton);
        islandStack.Children.Add(_islandMeasureButton);
        islandStack.Children.Add(_islandThemeButton);
        islandStack.Children.Add(_islandFitButton);
        islandStack.Children.Add(_navInfoButton);
        _floatingIslandBar.Content = islandStack;
        _canvasGrid.Children.Add(_floatingIslandBar);

        _viewerView.Add(_canvasGrid, 0, 0);

        // --- MODERN BOTTOM SHEET: KATMANLAR (LAYERS) ---
        _layerModalView = new Grid
        {
            BackgroundColor = Color.FromArgb("#80000000"),
            IsVisible = false
        };
        var layerScrimTap = new TapGestureRecognizer();
        layerScrimTap.Tapped += (_, _) => _layerModalView.IsVisible = false;
        _layerModalView.GestureRecognizers.Add(layerScrimTap);

        var layerSheetCard = new Border
        {
            Stroke = Color.FromArgb("#334155"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24, 24, 0, 0) },
            BackgroundColor = ColorBgSurface,
            Padding = new Thickness(20, 12, 20, 24),
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Fill,
            MaximumHeightRequest = 520,
            Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.5f, Radius = 20, Offset = new Point(0, -4) }
        };

        var layerCardStack = new VerticalStackLayout { Spacing = 10 };

        var dragPill = new Border
        {
            BackgroundColor = Color.FromArgb("#475569"),
            StrokeShape = new RoundRectangle { CornerRadius = 2 },
            WidthRequest = 40,
            HeightRequest = 4,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 4)
        };
        layerCardStack.Children.Add(dragPill);

        var layerHeaderRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        layerHeaderRow.Add(new Label
        {
            Text = "📑 Çizim Katmanları (Layers)",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = ColorTextPrimary,
            VerticalOptions = LayoutOptions.Center
        }, 0, 0);

        var closeLayerBtn = new Button
        {
            Text = "✕",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = ColorTextSecondary,
            BackgroundColor = Color.FromArgb("#1E293B"),
            CornerRadius = 16,
            WidthRequest = 32,
            HeightRequest = 32,
            Padding = 0
        };
        closeLayerBtn.Clicked += (_, _) => _layerModalView.IsVisible = false;
        layerHeaderRow.Add(closeLayerBtn, 1, 0);
        layerCardStack.Children.Add(layerHeaderRow);

        var quickFilterRow = new HorizontalStackLayout { Spacing = 8 };
        var btnAllVisible = new Button
        {
            Text = "👁️ Tümünü Aç",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = ColorBorderHighlight,
            BackgroundColor = Color.FromArgb("#1E293B"),
            CornerRadius = 12,
            HeightRequest = 30,
            Padding = new Thickness(10, 0)
        };
        btnAllVisible.Clicked += (_, _) =>
        {
            if (_currentScene is null) return;
            foreach (var l in _currentScene.LayerTable.Layers)
            {
                _currentScene.LayerTable.SetLayerVisibility(l.Name, true);
                _session?.SetLayerVisibility(l.Name, true);
            }
            OpenLayerModal();
            _viewportView.RequestFrame();
        };

        var btnAllHidden = new Button
        {
            Text = "🚫 Tümünü Kapat",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#F87171"),
            BackgroundColor = Color.FromArgb("#1E293B"),
            CornerRadius = 12,
            HeightRequest = 30,
            Padding = new Thickness(10, 0)
        };
        btnAllHidden.Clicked += (_, _) =>
        {
            if (_currentScene is null) return;
            foreach (var l in _currentScene.LayerTable.Layers)
            {
                _currentScene.LayerTable.SetLayerVisibility(l.Name, false);
                _session?.SetLayerVisibility(l.Name, false);
            }
            OpenLayerModal();
            _viewportView.RequestFrame();
        };

        quickFilterRow.Children.Add(btnAllVisible);
        quickFilterRow.Children.Add(btnAllHidden);
        layerCardStack.Children.Add(quickFilterRow);

        _layerStackLayout = new VerticalStackLayout { Spacing = 4 };
        var layerScroll = new ScrollView
        {
            Content = _layerStackLayout,
            MaximumHeightRequest = 300
        };
        layerCardStack.Children.Add(layerScroll);

        layerSheetCard.Content = layerCardStack;
        _layerModalView.Children.Add(layerSheetCard);

        // --- MODERN BOTTOM SHEET: BİLGİ VE TEŞHİS ---
        _infoModalView = new Grid
        {
            BackgroundColor = Color.FromArgb("#80000000"),
            IsVisible = false
        };
        var infoScrimTap = new TapGestureRecognizer();
        infoScrimTap.Tapped += (_, _) => _infoModalView.IsVisible = false;
        _infoModalView.GestureRecognizers.Add(infoScrimTap);

        var infoSheetCard = new Border
        {
            Stroke = Color.FromArgb("#334155"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24, 24, 0, 0) },
            BackgroundColor = ColorBgSurface,
            Padding = new Thickness(20, 12, 20, 24),
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Fill,
            MaximumHeightRequest = 520,
            Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.5f, Radius = 20, Offset = new Point(0, -4) }
        };

        var infoCardStack = new VerticalStackLayout { Spacing = 12 };

        var dragPillInfo = new Border
        {
            BackgroundColor = Color.FromArgb("#475569"),
            StrokeShape = new RoundRectangle { CornerRadius = 2 },
            WidthRequest = 40,
            HeightRequest = 4,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 4)
        };
        infoCardStack.Children.Add(dragPillInfo);

        var infoHeaderRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        infoHeaderRow.Add(new Label
        {
            Text = "ℹ️ Çizim Bilgileri ve Teşhis",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = ColorTextPrimary,
            VerticalOptions = LayoutOptions.Center
        }, 0, 0);

        var closeInfoBtn = new Button
        {
            Text = "✕",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = ColorTextSecondary,
            BackgroundColor = Color.FromArgb("#1E293B"),
            CornerRadius = 16,
            WidthRequest = 32,
            HeightRequest = 32,
            Padding = 0
        };
        closeInfoBtn.Clicked += (_, _) => _infoModalView.IsVisible = false;
        infoHeaderRow.Add(closeInfoBtn, 1, 0);
        infoCardStack.Children.Add(infoHeaderRow);

        _infoContentLabel = new Label
        {
            FontSize = 12,
            TextColor = Color.FromArgb("#E2E8F0"),
            LineHeight = 1.4
        };
        infoCardStack.Children.Add(_infoContentLabel);

        infoSheetCard.Content = infoCardStack;
        _infoModalView.Children.Add(infoSheetCard);

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

    private static Border CreateSampleCard(
        string icon,
        string title,
        string subtitle,
        string badge,
        Action onTap,
        string? automationId = null)
    {
        var border = new Border
        {
            AutomationId = automationId,
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
            TextColor = ColorBorderHighlight,
            BackgroundColor = Color.FromArgb("#EA0F172A"),
            CornerRadius = 24,
            WidthRequest = 48,
            HeightRequest = 48,
            Padding = 0
        };
        btn.Clicked += (_, _) => onClick();
        return btn;
    }

    private static Button CreateIslandButton(string text, Color textColor, Action onClick)
    {
        var btn = new Button
        {
            Text = text,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = textColor,
            BackgroundColor = Color.FromArgb("#263345"),
            CornerRadius = 18,
            HeightRequest = 36,
            Padding = new Thickness(12, 0)
        };
        btn.Clicked += (_, _) => onClick();
        return btn;
    }

    private void ToggleMeasureMode()
    {
        _isMeasureMode = !_isMeasureMode;
        if (_session is not null)
        {
            _session.InteractionEngine.IsMeasurementMode = _isMeasureMode;
        }
        _measureStartPoint = null;
        if (_measureHud is not null)
        {
            _measureHud.IsVisible = _isMeasureMode;
        }
        if (_measureLabel is not null)
        {
            _measureLabel.Text = "📏 Ölçüm Modu: 1. Noktaya dokunun...";
        }
        if (_islandMeasureButton is not null)
        {
            _islandMeasureButton.BackgroundColor = _isMeasureMode ? ColorAccentAmber : Color.FromArgb("#263345");
            _islandMeasureButton.TextColor = _isMeasureMode ? Colors.Black : ColorAccentAmber;
        }
    }

    private void OnSingleTap(ScreenPoint2 screenPoint)
    {
        if (!_isMeasureMode || _session is null || _currentScene is null) return;

        var camera = _session.Camera;
        double density = GetDensity();
        var (snapped, snapPoint, _) = SnapToNearestVertex(_currentScene, camera, screenPoint, 28.0 * density);
        var worldPt = snapped ? snapPoint : CameraTransform.ScreenToWorld(screenPoint, camera);

        if (_measureStartPoint is null)
        {
            _measureStartPoint = worldPt;
            if (_measureLabel is not null)
            {
                _measureLabel.Text = $"📏 1. Nokta ({worldPt.X:F1}, {worldPt.Y:F1}). 2. Noktaya dokunun...";
            }
        }
        else
        {
            double dx = worldPt.X - _measureStartPoint.Value.X;
            double dy = worldPt.Y - _measureStartPoint.Value.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            string distText = dist >= 100.0 ? $"{dist / 100.0:F2} m" : $"{dist:F1} cm";
            if (_measureLabel is not null)
            {
                _measureLabel.Text = $"📐 Ölçülen: {distText} (dx={dx:F1}, dy={dy:F1})";
            }
            _measureStartPoint = null;
        }
    }

    private async Task ToggleThemeModeAsync()
    {
        _isLightMode = !_isLightMode;
        if (_islandThemeButton is not null)
        {
            _islandThemeButton.Text = _isLightMode ? "🌙 Koyu" : "☀️ Tema";
        }
        if (_canvasGrid is not null)
        {
            _canvasGrid.BackgroundColor = _isLightMode ? Color.FromArgb("#F8FAFC") : ColorBgCanvas;
        }
        if (_currentScene is not null && _session is not null)
        {
            var newColorContext = _isLightMode ? RenderColorContext.Light : RenderColorContext.Dark;
            _currentScene = new RenderScene(
                _currentScene.Entities,
                _currentScene.Diagnostics,
                newColorContext,
                _currentScene.LayerTable);

            var oldCamera = _session.Camera;
            var (pw, ph) = GetViewportPixelDimensions();
            var lm = new CadLayoutManager(_currentScene);

            var newSession = new CadViewerSession(_session.Metadata, _currentScene, lm, pw, ph);
            newSession.Controller.SetCamera(oldCamera);
            newSession.InteractionEngine.CameraChanged += _ => Dispatcher.Dispatch(UpdateHud);
            newSession.InteractionEngine.SingleTapDetected += pt => Dispatcher.Dispatch(() => OnSingleTap(pt));

            _session.Dispose();
            _session = newSession;
            _viewportView.BindSession(_session);
            _viewportView.RequestFrame();
        }
        await Task.CompletedTask;
    }

    private static (bool Snapped, WorldPoint2 WorldPoint, double Distance) SnapToNearestVertex(
        RenderScene? scene,
        Camera2D camera,
        ScreenPoint2 touchScreenPoint,
        double maxScreenDistance)
    {
        if (scene is null) return (false, default, double.MaxValue);

        bool found = false;
        WorldPoint2 bestWorldPoint = default;
        double bestScreenDist = maxScreenDistance;

        void CheckCandidate(WorldPoint2 wpt)
        {
            var sp = CameraTransform.WorldToScreen(wpt, camera);
            double dx = sp.X - touchScreenPoint.X;
            double dy = sp.Y - touchScreenPoint.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < bestScreenDist)
            {
                bestScreenDist = dist;
                bestWorldPoint = wpt;
                found = true;
            }
        }

        foreach (var entity in scene.Entities)
        {
            if (!scene.LayerTable.GetLayer(entity.Layer.Value).IsVisible) continue;

            foreach (var geom in entity.Geometry)
            {
                switch (geom)
                {
                    case PointPrimitive pt:
                        CheckCandidate(pt.Position);
                        break;
                    case LinePrimitive line:
                        CheckCandidate(line.Start);
                        CheckCandidate(line.End);
                        break;
                    case ArcPrimitive arc:
                        CheckCandidate(arc.Center);
                        CheckCandidate(new WorldPoint2(
                            arc.Center.X + arc.Radius * Math.Cos(arc.StartRadians),
                            arc.Center.Y + arc.Radius * Math.Sin(arc.StartRadians)));
                        CheckCandidate(new WorldPoint2(
                            arc.Center.X + arc.Radius * Math.Cos(arc.StartRadians + arc.SweepRadians),
                            arc.Center.Y + arc.Radius * Math.Sin(arc.StartRadians + arc.SweepRadians)));
                        break;
                    case EllipsePrimitive ellipse:
                        CheckCandidate(ellipse.Center);
                        break;
                    case PolylinePrimitive poly:
                        foreach (var v in poly.Vertices)
                        {
                            CheckCandidate(v.Position);
                        }
                        break;
                    case PolygonPrimitive polygon:
                        foreach (var v in polygon.Vertices)
                        {
                            CheckCandidate(v);
                        }
                        break;
                    case SplinePrimitive spline:
                        foreach (var cp in spline.ControlPoints)
                        {
                            CheckCandidate(cp);
                        }
                        break;
                }
            }
        }

        return (found, bestWorldPoint, bestScreenDist);
    }

    private static double GetDensity()
    {
        try
        {
            double density = DeviceDisplay.Current.MainDisplayInfo.Density;
            if (density > 0 && double.IsFinite(density)) return density;
        }
        catch { }
        return 1.0;
    }

    private (int pixelWidth, int pixelHeight) GetViewportPixelDimensions()
    {
        double density = GetDensity();
        double dipsW = _canvasGrid.Width;
        double dipsH = _canvasGrid.Height;

        if (dipsW <= 0 || !double.IsFinite(dipsW) || dipsH <= 0 || !double.IsFinite(dipsH))
        {
            try
            {
                var display = DeviceDisplay.Current.MainDisplayInfo;
                if (display.Width > 0 && display.Height > 0)
                {
                    return ((int)display.Width, (int)display.Height);
                }
            }
            catch { }
            return (1080, 1920);
        }

        int pw = (int)Math.Max(100, Math.Round(dipsW * density));
        int ph = (int)Math.Max(100, Math.Round(dipsH * density));
        return (pw, ph);
    }

    private async Task DisplayCadSceneAsync(RenderScene scene, string displayName, string version)
    {
        _transitionOverlay.IsVisible = true;
        _currentScene = scene;
        _activeDocumentName = displayName;
        _activeDocumentVersion = version;

        _isMeasureMode = false;
        _measureStartPoint = null;
        if (_measureHud is not null) _measureHud.IsVisible = false;
        if (_islandMeasureButton is not null)
        {
            _islandMeasureButton.BackgroundColor = Color.FromArgb("#263345");
            _islandMeasureButton.TextColor = ColorAccentAmber;
        }

        var (pw, ph) = GetViewportPixelDimensions();

        var format = displayName.EndsWith(".dxf", StringComparison.OrdinalIgnoreCase)
            ? CadFormat.Dxf
            : CadFormat.Dwg;
        var metadata = new CadDocumentMetadata(
            format,
            version,
            displayName);

        var layoutManager = new CadLayoutManager(scene);

        _session?.Dispose();
        _session = new CadViewerSession(metadata, scene, layoutManager, pw, ph);
        _session.InteractionEngine.CameraChanged += _ => Dispatcher.Dispatch(UpdateHud);
        _session.InteractionEngine.SingleTapDetected += pt => Dispatcher.Dispatch(() => OnSingleTap(pt));

        _viewportView.BindSession(_session);

        _viewerTitleLabel.Text = displayName;
        _viewerVersionBadge.Text = version.Contains("AC", StringComparison.OrdinalIgnoreCase) ? $"DWG {version}" : version;

        UpdateHud();

        _dashboardView.IsVisible = false;
        _viewerView.IsVisible = true;
        _navLayerButton.IsVisible = true;
        _navInfoButton.IsVisible = true;
        _navCloseButton.IsVisible = true;

        _viewportView.RequestFrame();
        await Task.CompletedTask;
    }

    private void UpdateHud()
    {
        if (_session is null) return;
        var camera = _session.Camera;
        var bounds = _session.Controller.SceneBounds ?? new WorldBounds2(0, 0, 100, 100);
        var fitCamera = Camera2D.Fit(bounds, camera.PixelWidth, camera.PixelHeight);
        double zoomPercent = Math.Round((fitCamera.WorldUnitsPerPixel / Math.Max(1e-9, camera.WorldUnitsPerPixel)) * 100.0);
        _zoomLabel.Text = $"🔎 %{zoomPercent:N0}";
        if (_statsLabel is not null && _currentScene is not null)
        {
            _statsLabel.Text = $"📦 {_currentScene.Entities.Count} Varlık  •  📑 {_currentScene.LayerTable.Layers.Count} Katman";
        }
    }

    private Task ReRenderAsync()
    {
        UpdateHud();
        _viewportView.RequestFrame();
        return Task.CompletedTask;
    }

    private void CloseActiveDrawing()
    {
        _session?.Dispose();
        _session = null;
        _viewportView.BindSession(null);
        _currentScene = null;
        _activeDocumentName = null;
        _activeDocumentVersion = null;
        _isMeasureMode = false;
        _measureStartPoint = null;
        if (_measureHud is not null) _measureHud.IsVisible = false;
        if (_islandMeasureButton is not null)
        {
            _islandMeasureButton.BackgroundColor = Color.FromArgb("#263345");
            _islandMeasureButton.TextColor = ColorAccentAmber;
        }

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
            var rowCard = new Border
            {
                Stroke = Color.FromArgb("#1E293B"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                BackgroundColor = Color.FromArgb("#162032"),
                Padding = new Thickness(12, 8),
                Margin = new Thickness(0, 2)
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

            var colorIndicator = new Border
            {
                Stroke = Color.FromArgb("#475569"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 6 },
                BackgroundColor = Color.FromUint(layer.Color.Argb),
                WidthRequest = 18,
                HeightRequest = 18,
                VerticalOptions = LayoutOptions.Center
            };
            row.Add(colorIndicator, 0, 0);

            var nameLabel = new Label
            {
                Text = layer.Name,
                TextColor = ColorTextPrimary,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation
            };
            row.Add(nameLabel, 1, 0);

            var toggle = new Switch
            {
                IsToggled = layer.IsVisible,
                OnColor = ColorAccentBlue,
                ThumbColor = Colors.White,
                VerticalOptions = LayoutOptions.Center
            };
            var capturedLayerName = layer.Name;
            toggle.Toggled += (_, te) =>
            {
                _currentScene.LayerTable.SetLayerVisibility(capturedLayerName, te.Value);
                _session?.SetLayerVisibility(capturedLayerName, te.Value);
                _viewportView.RequestFrame();
            };
            row.Add(toggle, 2, 0);

            rowCard.Content = row;
            _layerStackLayout.Children.Add(rowCard);
        }

        _layerModalView.IsVisible = true;
    }

    private void OpenInfoModal()
    {
        if (_currentScene is null) return;

        var bounds = _viewportController?.SceneBounds;
        var info = new System.Text.StringBuilder();
        info.AppendLine($"📄 Dosya Adı: {_activeDocumentName ?? "Bilinmiyor"}");
        info.AppendLine($"🔖 CAD Sürümü: {_activeDocumentVersion ?? "CAD Standart"}");
        info.AppendLine($"📦 Toplam Varlık Sayısı: {_currentScene.Entities.Count:N0}");
        info.AppendLine($"📑 Katman (Layer) Sayısı: {_currentScene.LayerTable.Layers.Count}");
        if (bounds.HasValue)
        {
            info.AppendLine($"📐 X Sınırları: {bounds.Value.MinX:F1} → {bounds.Value.MaxX:F1} ({bounds.Value.Width:F1} birim)");
            info.AppendLine($"📐 Y Sınırları: {bounds.Value.MinY:F1} → {bounds.Value.MaxY:F1} ({bounds.Value.Height:F1} birim)");
        }
        info.AppendLine($"🎨 Render Teması: {(_isLightMode ? "Gündüz / Sunlight" : "Karanlık / Dark Slate")}");
        info.AppendLine($"🔒 Güvenlik: %100 Çevrimdışı (Veriler cihazdan çıkmaz)");
        info.AppendLine($"⚡ Motor: Donanım Hızlandırmalı Skia Vektör Motoru");

        _infoContentLabel.Text = info.ToString();
        _infoModalView.IsVisible = true;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
#if V06_VALIDATION
        LogV06("V06_LIFECYCLE_APPEARING");
#endif
#if ANDROID
        try
        {
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            var intent = activity?.Intent;
            if (intent is not null)
            {
                var openCad = intent.GetStringExtra("open_cad");
                if (!string.IsNullOrEmpty(openCad))
                {
                    intent.RemoveExtra("open_cad");
                    Dispatcher.Dispatch(async () =>
                    {
                        await Task.Delay(400);
                        await OpenDesktopCadFileAsync(openCad, openCad);
                    });
                }
            }
        }
        catch { }
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

    public async Task OpenDesktopCadFileAsync(string displayName, params string[] candidateNames)
    {
        try
        {
            var searchDirs = new List<string>();
#if ANDROID
            try
            {
                var extFiles = Android.App.Application.Context.GetExternalFilesDir(null)?.AbsolutePath;
                if (!string.IsNullOrEmpty(extFiles))
                {
                    searchDirs.Add(extFiles);
                    searchDirs.Add(System.IO.Path.Combine(extFiles, "CAD"));
                }
            }
            catch { }
            try
            {
                var downloads = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
                if (!string.IsNullOrEmpty(downloads)) searchDirs.Add(downloads);
            }
            catch { }
#endif
            try
            {
                var appData = FileSystem.Current.AppDataDirectory;
                if (!string.IsNullOrEmpty(appData)) searchDirs.Add(appData);
            }
            catch { }
            try
            {
                var cacheDir = FileSystem.Current.CacheDirectory;
                if (!string.IsNullOrEmpty(cacheDir)) searchDirs.Add(cacheDir);
            }
            catch { }
            searchDirs.Add("/sdcard/Android/data/com.smitelagwar.mobildwg/files");
            searchDirs.Add("/sdcard/Android/data/com.smitelagwar.mobildwg/files/CAD");
            searchDirs.Add("/storage/emulated/0/Android/data/com.smitelagwar.mobildwg/files");
            searchDirs.Add("/sdcard/Download");
            searchDirs.Add("/storage/emulated/0/Download");

            string? foundPath = null;
            foreach (var dir in searchDirs)
            {
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) continue;
                foreach (var name in candidateNames)
                {
                    var full = System.IO.Path.Combine(dir, name);
                    if (File.Exists(full))
                    {
                        foundPath = full;
                        break;
                    }
                }
                if (foundPath != null) break;
            }

            if (foundPath is null)
            {
                _status.Text = $"Dosya bulunamadı: {displayName}";
#if ANDROID
                Android.Util.Log.Warn("MobilDwgCAD", $"DESKTOP_FILE_NOT_FOUND name={displayName}");
#endif
                return;
            }

#if ANDROID
            Android.Util.Log.Info("MobilDwgCAD", $"DESKTOP_FILE_OPEN_START name={displayName} path={foundPath}");
#endif
            _status.Text = $"Açılıyor: {displayName}...";
            var fileInfo = new FileInfo(foundPath);
            var selection = new CadFileSelection(
                displayName,
                fileInfo.Length,
                _ => ValueTask.FromResult<Stream>(File.OpenRead(foundPath))
            );

            await OpenSelectionAsync(selection);
#if ANDROID
            Android.Util.Log.Info("MobilDwgCAD", $"DESKTOP_FILE_OPEN_SUCCESS name={displayName}");
#endif
        }
        catch (Exception ex)
        {
            _status.Text = $"Açma hatası: {ex.Message}";
#if ANDROID
            Android.Util.Log.Error("MobilDwgCAD", $"DESKTOP_FILE_OPEN_FAIL name={displayName} ex={ex}");
#endif
        }
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
#if ANDROID
                    var swExt = System.Diagnostics.Stopwatch.StartNew();
#endif
                    var extracted = AcadSharpEntityExtractor.Extract(coordinator.CurrentSession.Handle);
#if ANDROID
                    swExt.Stop();
                    Android.Util.Log.Info("MobilDwgCAD", $"STAGE_EXTRACT_DONE count={extracted.Entities.Count} in {swExt.ElapsedMilliseconds}ms");
                    var swScn = System.Diagnostics.Stopwatch.StartNew();
#endif
                    var scene = CadExtractedSceneBuilder.Build(extracted);
#if ANDROID
                    swScn.Stop();
                    Android.Util.Log.Info("MobilDwgCAD", $"STAGE_SCENE_DONE entities={scene.Entities.Count} in {swScn.ElapsedMilliseconds}ms");
#endif
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
