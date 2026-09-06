namespace MobilDwg.Core.Diagnostics;

public static class LogRedactor
{
    public static string RedactPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "[EMPTY]";

        try
        {
            var fileName = GetPortableLeafName(path);
            if (!string.IsNullOrEmpty(fileName))
            {
                return fileName;
            }
        }
        catch
        {
            // fallback
        }

        return "[REDACTED_PATH]";
    }

    public static string RedactUri(string? uriString)
    {
        if (string.IsNullOrWhiteSpace(uriString)) return "[EMPTY]";

        try
        {
            if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            {
                var leaf = GetPortableLeafName(uri.LocalPath);
                if (!string.IsNullOrEmpty(leaf))
                {
                    return $"{uri.Scheme}://.../{leaf}";
                }
                return $"{uri.Scheme}://[REDACTED]";
            }
        }
        catch
        {
            // fallback
        }

        return "[REDACTED_URI]";
    }

    private static string? GetPortableLeafName(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0) return null;

        // Path.GetFileName follows the host OS separator rules. Logs can contain
        // paths originating from another platform, so treat both separators as
        // sensitive path boundaries regardless of where redaction executes.
        var separatorIndex = Math.Max(trimmed.LastIndexOf('/'), trimmed.LastIndexOf('\\'));
        if (separatorIndex >= 0)
        {
            if (separatorIndex == trimmed.Length - 1) return null;
            return trimmed[(separatorIndex + 1)..];
        }

        var fileName = Path.GetFileName(trimmed);
        return string.IsNullOrWhiteSpace(fileName) ? null : fileName;
    }
}
