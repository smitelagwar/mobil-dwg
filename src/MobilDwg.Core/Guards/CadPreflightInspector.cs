using System.Text;
using System.Text.RegularExpressions;
using MobilDwg.Core.Documents;

namespace MobilDwg.Core.Guards;

public enum CadPreflightStatus
{
    Valid = 0,
    EmptyOrTruncated = 1,
    InvalidDwgMagic = 2,
    InvalidDxfStructure = 3,
    ForeignFormat = 4,
    UnrecognizedFormat = 5,
}

public sealed record CadPreflightResult(
    CadPreflightStatus Status,
    CadFormat? Format,
    string? Version,
    string DiagnosticCode,
    string Message,
    long StreamLength);

public static class CadPreflightInspector
{
    private static readonly Regex AcadVersionRegex = new(
        @"\$ACADVER\s*\r?\n\s*1\s*\r?\n\s*(AC\d{4})",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static CadPreflightResult Inspect(Stream stream, string? displayName = null)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            return new CadPreflightResult(
                CadPreflightStatus.EmptyOrTruncated,
                null,
                null,
                "CAD_STREAM_UNREADABLE",
                "Stream cannot be read.",
                0);
        }

        long streamLength = -1;
        if (stream.CanSeek)
        {
            streamLength = stream.Length;
            if (streamLength == 0)
            {
                return new CadPreflightResult(
                    CadPreflightStatus.EmptyOrTruncated,
                    null,
                    null,
                    "CAD_EMPTY_STREAM",
                    "Input stream contains zero bytes.",
                    0);
            }
            if (streamLength < 6)
            {
                return new CadPreflightResult(
                    CadPreflightStatus.EmptyOrTruncated,
                    null,
                    null,
                    "CAD_TRUNCATED_HEADER",
                    $"Input stream is too small for a valid CAD header ({streamLength} bytes).",
                    streamLength);
            }
        }

        long originalPosition = 0;
        if (stream.CanSeek)
        {
            originalPosition = stream.Position;
            stream.Position = 0;
        }

        byte[] headerBuffer = new byte[Math.Min(stream.CanSeek ? (int)Math.Min(streamLength, 4096) : 4096, 4096)];
        int bytesRead = 0;
        try
        {
            while (bytesRead < headerBuffer.Length)
            {
                int count = stream.Read(headerBuffer, bytesRead, headerBuffer.Length - bytesRead);
                if (count == 0)
                {
                    break;
                }
                bytesRead += count;
            }
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }

        if (bytesRead < 6)
        {
            return new CadPreflightResult(
                CadPreflightStatus.EmptyOrTruncated,
                null,
                null,
                "CAD_TRUNCATED_HEADER",
                $"Could only read {bytesRead} bytes; minimal CAD header requires at least 6 bytes.",
                streamLength >= 0 ? streamLength : bytesRead);
        }

        ReadOnlySpan<byte> headerSpan = headerBuffer.AsSpan(0, bytesRead);

        // Foreign binary file signatures
        if (headerSpan.Length >= 2 && headerSpan[0] == (byte)'M' && headerSpan[1] == (byte)'Z')
        {
            return new CadPreflightResult(
                CadPreflightStatus.ForeignFormat,
                null,
                null,
                "CAD_FOREIGN_FORMAT_PE_EXECUTABLE",
                "Input is a Windows PE executable file, not a CAD drawing.",
                streamLength);
        }

        if (headerSpan.Length >= 4 && headerSpan[0] == 0x7F && headerSpan[1] == (byte)'E' && headerSpan[2] == (byte)'L' && headerSpan[3] == (byte)'F')
        {
            return new CadPreflightResult(
                CadPreflightStatus.ForeignFormat,
                null,
                null,
                "CAD_FOREIGN_FORMAT_ELF_EXECUTABLE",
                "Input is a Linux ELF binary, not a CAD drawing.",
                streamLength);
        }

        if (headerSpan.Length >= 4 && headerSpan[0] == (byte)'P' && headerSpan[1] == (byte)'K' && headerSpan[2] == 0x03 && headerSpan[3] == 0x04)
        {
            return new CadPreflightResult(
                CadPreflightStatus.ForeignFormat,
                null,
                null,
                "CAD_FOREIGN_FORMAT_ZIP_ARCHIVE",
                "Input is a ZIP archive or package, not a CAD drawing.",
                streamLength);
        }

        if (headerSpan.Length >= 4 && headerSpan[0] == (byte)'%' && headerSpan[1] == (byte)'P' && headerSpan[2] == (byte)'D' && headerSpan[3] == (byte)'F')
        {
            return new CadPreflightResult(
                CadPreflightStatus.ForeignFormat,
                null,
                null,
                "CAD_FOREIGN_FORMAT_PDF_DOCUMENT",
                "Input is a PDF document, not a CAD drawing.",
                streamLength);
        }

        if (headerSpan.Length >= 4 && headerSpan[0] == 0x89 && headerSpan[1] == (byte)'P' && headerSpan[2] == (byte)'N' && headerSpan[3] == (byte)'G')
        {
            return new CadPreflightResult(
                CadPreflightStatus.ForeignFormat,
                null,
                null,
                "CAD_FOREIGN_FORMAT_PNG_IMAGE",
                "Input is a PNG image, not a CAD drawing.",
                streamLength);
        }

        if (headerSpan.Length >= 3 && headerSpan[0] == 0xFF && headerSpan[1] == 0xD8 && headerSpan[2] == 0xFF)
        {
            return new CadPreflightResult(
                CadPreflightStatus.ForeignFormat,
                null,
                null,
                "CAD_FOREIGN_FORMAT_JPEG_IMAGE",
                "Input is a JPEG image, not a CAD drawing.",
                streamLength);
        }

        if (headerSpan.Length >= 2 && headerSpan[0] == (byte)'B' && headerSpan[1] == (byte)'M')
        {
            return new CadPreflightResult(
                CadPreflightStatus.ForeignFormat,
                null,
                null,
                "CAD_FOREIGN_FORMAT_BMP_IMAGE",
                "Input is a BMP bitmap, not a CAD drawing.",
                streamLength);
        }

        // DWG Magic validation: starts with "AC10" or "AC" + 4 digits
        string prefixAscii = Encoding.ASCII.GetString(headerBuffer, 0, 6);
        if (prefixAscii.StartsWith("AC", StringComparison.Ordinal)
            && prefixAscii.Length == 6
            && prefixAscii.AsSpan(2).ToString().All(char.IsDigit))
        {
            return new CadPreflightResult(
                CadPreflightStatus.Valid,
                CadFormat.Dwg,
                prefixAscii,
                "CAD_PREFLIGHT_DWG_VALID",
                $"Valid AutoCAD DWG header ({prefixAscii}).",
                streamLength);
        }

        // If file extension is .dwg but magic does not match
        string ext = Path.GetExtension(displayName ?? string.Empty);
        if (ext.Equals(".dwg", StringComparison.OrdinalIgnoreCase))
        {
            return new CadPreflightResult(
                CadPreflightStatus.InvalidDwgMagic,
                CadFormat.Dwg,
                null,
                "CAD_INVALID_DWG_MAGIC",
                $"DWG file magic header is invalid (found '{prefixAscii.Replace("\0", "\\0")}').",
                streamLength);
        }

        // DXF Binary check
        if (headerSpan.StartsWith("AutoCAD Binary DXF"u8))
        {
            return new CadPreflightResult(
                CadPreflightStatus.Valid,
                CadFormat.Dxf,
                "BinaryDXF",
                "CAD_PREFLIGHT_DXF_BINARY_VALID",
                "Valid AutoCAD Binary DXF signature.",
                streamLength);
        }

        // DXF ASCII check
        string textHeader = Encoding.Latin1.GetString(headerBuffer, 0, bytesRead);

        // Check HTML
        if (textHeader.Contains("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase)
            || textHeader.Contains("<html", StringComparison.OrdinalIgnoreCase))
        {
            return new CadPreflightResult(
                CadPreflightStatus.ForeignFormat,
                null,
                null,
                "CAD_FOREIGN_FORMAT_HTML",
                "Input is an HTML document, not a CAD drawing.",
                streamLength);
        }

        var match = AcadVersionRegex.Match(textHeader);
        if (match.Success && textHeader.Contains("SECTION", StringComparison.Ordinal))
        {
            return new CadPreflightResult(
                CadPreflightStatus.Valid,
                CadFormat.Dxf,
                match.Groups[1].Value,
                "CAD_PREFLIGHT_DXF_ASCII_VALID",
                $"Valid AutoCAD ASCII DXF header ({match.Groups[1].Value}).",
                streamLength);
        }

        if (textHeader.Contains("SECTION", StringComparison.Ordinal)
            || textHeader.StartsWith("0\r\nSECTION", StringComparison.Ordinal)
            || textHeader.StartsWith("0\nSECTION", StringComparison.Ordinal)
            || textHeader.StartsWith("999\r\n", StringComparison.Ordinal)
            || textHeader.StartsWith("999\n", StringComparison.Ordinal))
        {
            return new CadPreflightResult(
                CadPreflightStatus.Valid,
                CadFormat.Dxf,
                null,
                "CAD_PREFLIGHT_DXF_SECTION_VALID",
                "Valid AutoCAD DXF section structure.",
                streamLength);
        }

        if (ext.Equals(".dxf", StringComparison.OrdinalIgnoreCase))
        {
            return new CadPreflightResult(
                CadPreflightStatus.InvalidDxfStructure,
                CadFormat.Dxf,
                null,
                "CAD_INVALID_DXF_STRUCTURE",
                "DXF file does not contain a recognizable SECTION or header.",
                streamLength);
        }

        return new CadPreflightResult(
            CadPreflightStatus.UnrecognizedFormat,
            null,
            null,
            "CAD_UNRECOGNIZED_FORMAT",
            "Input stream does not match any known CAD format or recognized signature.",
            streamLength);
    }
}
