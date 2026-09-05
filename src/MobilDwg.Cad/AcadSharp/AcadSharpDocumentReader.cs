using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using ACadSharp;
using ACadSharp.IO;
using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;

namespace MobilDwg.Cad.AcadSharp;

public sealed class AcadSharpDocumentReader : ICadDocumentReader
{
    private static readonly Regex AcadVersionRegex = new(
        @"\$ACADVER\s*\r?\n\s*1\s*\r?\n\s*(AC\d{4})",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public CadReaderCapabilities Capabilities { get; } = new(
        CancellationSupport.BeforeStartOnly,
        ProgressSupport.StagesOnly);

    public async ValueTask<CadDocumentSession> OpenAsync(
        CadOpenRequest request,
        IProgress<CadReadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Source);

        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(new CadReadProgress(CadReadStage.Preflight, message: "Inspecting CAD format and version."));

        var prepared = await PrepareStreamAsync(request.Source, request.LeaveOpen, cancellationToken).ConfigureAwait(false);
        try
        {
            var preflight = Inspect(prepared.Stream, request.DisplayName);
            cancellationToken.ThrowIfCancellationRequested();

            progress?.Report(new CadReadProgress(CadReadStage.Opening, message: $"Opening {preflight.Format}."));

            var diagnostics = new List<CadDiagnostic>();
            var compatibility = new List<CadCompatibilityIssue>();
            var stopwatch = Stopwatch.StartNew();

            CadDocument document;
            progress?.Report(new CadReadProgress(CadReadStage.Parsing, message: "ACadSharp parser running; cancellation is not cooperative after this point."));
            using (ICadReader reader = CreateReader(preflight.Format, prepared.Stream, request.LeaveOpen || prepared.OwnsBuffer))
            {
                reader.OnNotification += (_, args) => CaptureNotification(args, diagnostics, compatibility);
                document = reader.Read();
            }

            stopwatch.Stop();
            if (prepared.OwnsBuffer)
            {
                prepared.Stream.Dispose();
            }

            progress?.Report(new CadReadProgress(CadReadStage.Normalizing, message: "Collecting parser diagnostics and document metadata."));

            CollectCompatibility(document, compatibility);
            diagnostics.Add(new CadDiagnostic(
                "acadsharp.parse.timing",
                DiagnosticSeverity.Info,
                $"ACadSharp parse completed in {stopwatch.Elapsed.TotalMilliseconds:F1} ms."));

            var metadata = new CadDocumentMetadata(
                preflight.Format,
                document.Header?.Version.ToString() ?? preflight.AcadVersion,
                request.DisplayName);

            var handle = new AcadSharpDocumentHandle(document, stopwatch.Elapsed);
            progress?.Report(new CadReadProgress(CadReadStage.Completed, message: "CAD document parsed."));
            return new CadDocumentSession(handle, metadata, diagnostics, DistinctCompatibility(compatibility));
        }
        catch
        {
            if (prepared.OwnsBuffer)
            {
                prepared.Stream.Dispose();
            }

            throw;
        }
    }

    private static ICadReader CreateReader(CadFormat format, Stream stream, bool leaveOpen)
    {
        Stream source = leaveOpen ? new NonDisposingStream(stream) : stream;
        return format switch
        {
            CadFormat.Dwg => new DwgReader(source),
            CadFormat.Dxf => new DxfReader(source),
            _ => throw new InvalidDataException("Unsupported CAD format."),
        };
    }

    private static CadPreflightResult Inspect(Stream stream, string? displayName)
    {
        if (!stream.CanRead || !stream.CanSeek)
        {
            throw new InvalidDataException("ACadSharp adapter requires a readable, seekable stream after preparation.");
        }

        var original = stream.Position;
        try
        {
            stream.Position = 0;
            var length = (int)Math.Min(stream.Length, 65_536);
            var prefix = new byte[length];
            var read = 0;
            while (read < prefix.Length)
            {
                var count = stream.Read(prefix, read, prefix.Length - read);
                if (count == 0)
                {
                    break;
                }

                read += count;
            }

            if (read >= 6)
            {
                var dwgMagic = Encoding.ASCII.GetString(prefix, 0, 6);
                if (dwgMagic.StartsWith("AC", StringComparison.Ordinal)
                    && dwgMagic.AsSpan(2).ToString().All(char.IsDigit))
                {
                    return new CadPreflightResult(CadFormat.Dwg, dwgMagic, "dwg-magic");
                }
            }

            if (prefix.AsSpan(0, read).StartsWith("AutoCAD Binary DXF"u8))
            {
                return new CadPreflightResult(CadFormat.Dxf, null, "binary-dxf-signature");
            }

            var text = Encoding.Latin1.GetString(prefix, 0, read);
            var match = AcadVersionRegex.Match(text);
            if (match.Success && text.Contains("SECTION", StringComparison.Ordinal))
            {
                return new CadPreflightResult(CadFormat.Dxf, match.Groups[1].Value, "ascii-dxf-header");
            }

            var extension = Path.GetExtension(displayName ?? string.Empty);
            if (extension.Equals(".dxf", StringComparison.OrdinalIgnoreCase)
                && text.Contains("SECTION", StringComparison.Ordinal))
            {
                return new CadPreflightResult(CadFormat.Dxf, null, "ascii-dxf-structure-extension-assisted");
            }

            throw new InvalidDataException("Input does not expose a recognized DWG magic or DXF signature/header.");
        }
        finally
        {
            stream.Position = original;
        }
    }

    private static async ValueTask<PreparedStream> PrepareStreamAsync(
        Stream source,
        bool leaveOpen,
        CancellationToken cancellationToken)
    {
        if (source.CanSeek)
        {
            return new PreparedStream(source, false);
        }

        var buffer = new MemoryStream();
        try
        {
            await source.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
            buffer.Position = 0;
            if (!leaveOpen)
            {
                source.Dispose();
            }

            return new PreparedStream(buffer, true);
        }
        catch
        {
            buffer.Dispose();
            throw;
        }
    }

    private static void CaptureNotification(
        NotificationEventArgs args,
        ICollection<CadDiagnostic> diagnostics,
        ICollection<CadCompatibilityIssue> compatibility)
    {
        if (diagnostics.Count >= 50)
        {
            return;
        }

        var typeName = args.NotificationType.ToString();
        var severity = typeName.Contains("Error", StringComparison.OrdinalIgnoreCase)
            ? DiagnosticSeverity.Error
            : typeName.Contains("Warning", StringComparison.OrdinalIgnoreCase)
                ? DiagnosticSeverity.Warning
                : DiagnosticSeverity.Info;

        if (diagnostics.Count < 200)
        {
            var codeSuffix = string.Concat(typeName.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-')).Trim('-');
            if (string.IsNullOrEmpty(codeSuffix))
            {
                codeSuffix = "notice";
            }

            diagnostics.Add(new CadDiagnostic(
                $"acadsharp.notification.{codeSuffix}",
                severity,
                args.Message));
        }

        if (compatibility.Count < 100)
        {
            ClassifyMessage(args.Message, compatibility);
        }
    }

    private static void CollectCompatibility(
        CadDocument document,
        ICollection<CadCompatibilityIssue> compatibility)
    {
        foreach (var style in document.TextStyles)
        {
            var filename = style.Filename ?? string.Empty;
            if (filename.EndsWith(".shx", StringComparison.OrdinalIgnoreCase) && !File.Exists(filename))
            {
                compatibility.Add(new CadCompatibilityIssue(
                    CompatibilityIssueKind.MissingFont,
                    "missing-font",
                    $"SHX font resource is not available locally: {filename}",
                    filename));
            }
        }

        foreach (var block in document.BlockRecords)
        {
            var xref = block.BlockEntity?.XRefPath;
            if (!string.IsNullOrWhiteSpace(xref) && !File.Exists(xref))
            {
                compatibility.Add(new CadCompatibilityIssue(
                    CompatibilityIssueKind.MissingExternalReference,
                    "missing-xref",
                    $"External reference is not available locally: {xref}",
                    xref));
            }
        }
    }

    private static void ClassifyMessage(string? message, ICollection<CadCompatibilityIssue> compatibility)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (message.Contains("proxy", StringComparison.OrdinalIgnoreCase))
        {
            compatibility.Add(new CadCompatibilityIssue(
                CompatibilityIssueKind.ProxyObject,
                "proxy-object",
                message));
        }

        if (message.Contains("not supported", StringComparison.OrdinalIgnoreCase)
            || message.Contains("unsupported", StringComparison.OrdinalIgnoreCase))
        {
            compatibility.Add(new CadCompatibilityIssue(
                CompatibilityIssueKind.UnsupportedEntity,
                "unsupported-object",
                message));
        }

        if (message.Contains("font", StringComparison.OrdinalIgnoreCase)
            && (message.Contains("missing", StringComparison.OrdinalIgnoreCase)
                || message.Contains("not found", StringComparison.OrdinalIgnoreCase)))
        {
            compatibility.Add(new CadCompatibilityIssue(
                CompatibilityIssueKind.MissingFont,
                "missing-font",
                message));
        }

        if ((message.Contains("xref", StringComparison.OrdinalIgnoreCase)
                || message.Contains("external reference", StringComparison.OrdinalIgnoreCase))
            && (message.Contains("missing", StringComparison.OrdinalIgnoreCase)
                || message.Contains("not found", StringComparison.OrdinalIgnoreCase)))
        {
            compatibility.Add(new CadCompatibilityIssue(
                CompatibilityIssueKind.MissingExternalReference,
                "missing-xref",
                message));
        }
    }

    private static IReadOnlyList<CadCompatibilityIssue> DistinctCompatibility(IEnumerable<CadCompatibilityIssue> issues) =>
        issues
            .GroupBy(issue => (issue.Kind, issue.Code, issue.Resource, issue.Message))
            .Select(group => group.First())
            .ToArray();

    private sealed record CadPreflightResult(CadFormat Format, string? AcadVersion, string Evidence);

    private readonly record struct PreparedStream(Stream Stream, bool OwnsBuffer);
}

public sealed class AcadSharpDocumentHandle : ICadDocumentHandle
{
    private CadDocument? _document;

    internal AcadSharpDocumentHandle(CadDocument document, TimeSpan parseDuration)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        ParseDuration = parseDuration;
    }

    internal CadDocument Document =>
        Volatile.Read(ref _document) ?? throw new ObjectDisposedException(nameof(AcadSharpDocumentHandle));

    public TimeSpan ParseDuration { get; }

    public ValueTask DisposeAsync()
    {
        Interlocked.Exchange(ref _document, null);
        return ValueTask.CompletedTask;
    }
}

public sealed record AcadSharpParserSnapshot(
    string AcadVersion,
    int LayerCount,
    int BlockCount,
    int LayoutCount,
    int ModelSpaceEntityCount,
    int TotalBlockEntityCount,
    IReadOnlyDictionary<string, int> EntityCounts,
    double ParseMilliseconds);

public static class AcadSharpDocumentInspection
{
    public static AcadSharpParserSnapshot Snapshot(ICadDocumentHandle handle)
    {
        if (handle is not AcadSharpDocumentHandle acadHandle)
        {
            throw new ArgumentException("Handle was not created by the ACadSharp adapter.", nameof(handle));
        }

        var document = acadHandle.Document;
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var total = 0;

        foreach (var block in document.BlockRecords)
        {
            foreach (var entity in block.Entities)
            {
                total++;
                Increment(counts, entity.ObjectName);
                if (string.Equals(entity.ObjectName, "INSERT", StringComparison.OrdinalIgnoreCase))
                {
                    Increment(counts, "BLOCK_REFERENCE");
                }
            }
        }

        return new AcadSharpParserSnapshot(
            document.Header?.Version.ToString() ?? "Unknown",
            document.Layers.Count(),
            document.BlockRecords.Count(),
            document.Layouts?.Count() ?? 0,
            document.Entities.Count,
            total,
            counts,
            acadHandle.ParseDuration.TotalMilliseconds);
    }

    private static void Increment(IDictionary<string, int> counts, string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            key = "<UNKNOWN>";
        }

        counts.TryGetValue(key, out var current);
        counts[key] = current + 1;
    }
}

internal sealed class NonDisposingStream : Stream
{
    private readonly Stream _inner;

    public NonDisposingStream(Stream inner) => _inner = inner;

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _inner.Length;
    public override long Position { get => _inner.Position; set => _inner.Position = value; }
    public override void Flush() => _inner.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    public override int Read(Span<byte> buffer) => _inner.Read(buffer);
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    protected override void Dispose(bool disposing) { }
}
