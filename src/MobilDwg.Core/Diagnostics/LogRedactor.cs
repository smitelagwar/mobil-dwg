namespace MobilDwg.Core.Diagnostics;

public static class LogRedactor
{
    public static string RedactPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "[EMPTY]";

        try
        {
            var fileName = Path.GetFileName(path);
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
                var leaf = Path.GetFileName(uri.LocalPath);
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
}
