using System.Security.Cryptography;
using Foundation;
using MobilDwg.Cad.AcadSharp;
using MobilDwg.Core.Reading;
using SkiaSharp;
using UIKit;

namespace MobilDwg.Stage08.iOS;

public static class Application
{
    public static void Main(string[] args) => UIApplication.Main(args, null, typeof(AppDelegate));
}

[Register("AppDelegate")]
public sealed class AppDelegate : UIApplicationDelegate
{
    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
    {
        _ = Task.Run(RunProbe);
        return true;
    }

    private static void RunProbe()
    {
        try
        {
            var fixturePath = NSBundle.MainBundle.PathForResource("stage08_fixture", "dxf")
                ?? throw new FileNotFoundException("Bundled Stage 08 DXF fixture was not found.");

            using var fixture = File.OpenRead(fixturePath);
            var reader = new AcadSharpDocumentReader();
            var session = reader.OpenAsync(new CadOpenRequest(fixture, "stage08_fixture.dxf", fixture.Length, LeaveOpen: false))
                .AsTask().GetAwaiter().GetResult();

            try
            {
                var snapshot = AcadSharpDocumentInspection.Snapshot(session.Handle);
                if (!snapshot.EntityCounts.TryGetValue("LINE", out var lineCount) || lineCount != 2)
                {
                    throw new InvalidDataException($"Expected 2 LINE entities, observed {lineCount}.");
                }

                if (!snapshot.EntityCounts.TryGetValue("CIRCLE", out var circleCount) || circleCount != 1)
                {
                    throw new InvalidDataException($"Expected 1 CIRCLE entity, observed {circleCount}.");
                }

                Console.WriteLine($"STAGE08_IOS_SIMULATOR_PARSE_PASS version={snapshot.AcadVersion} total={snapshot.TotalBlockEntityCount}");
            }
            finally
            {
                session.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }

            using var bitmap = new SKBitmap(96, 96, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);
            using var paint = new SKPaint { IsAntialias = true, StrokeWidth = 3, Style = SKPaintStyle.Stroke, Color = SKColors.Black };
            canvas.DrawLine(8, 12, 88, 84, paint);
            canvas.DrawCircle(48, 48, 18, paint);
            canvas.Flush();

            using var image = SKImage.FromBitmap(bitmap);
            using var encoded = image.Encode(SKEncodedImageFormat.Png, 100)
                ?? throw new InvalidOperationException("SkiaSharp PNG encode returned null.");
            var pngBytes = encoded.ToArray();
            var hash = Convert.ToHexString(SHA256.HashData(pngBytes)).ToLowerInvariant();
            Console.WriteLine($"STAGE08_IOS_SIMULATOR_SKIA_PASS bytes={pngBytes.Length} sha256={hash}");
            Console.WriteLine("STAGE08_IOS_SIMULATOR_SMOKE_PASS");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"STAGE08_IOS_SIMULATOR_SMOKE_FAIL {ex.GetType().Name}: {ex.Message}");
            Console.Error.WriteLine(ex);
            Environment.Exit(2);
        }
    }
}
