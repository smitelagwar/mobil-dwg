using System.Diagnostics.CodeAnalysis;

namespace MobilDwg.Rendering.References;

public static class CadReferenceResolver
{
    private static readonly string[] RemoteSchemes = ["http://", "https://", "ftp://", "ftps://", "cloud://"];

    public static bool IsRemoteUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        return RemoteSchemes.Any(scheme => path.StartsWith(scheme, StringComparison.OrdinalIgnoreCase));
    }

    public static bool TryResolve(
        string rawPath,
        IEnumerable<string>? searchDirectories,
        [NotNullWhen(true)] out string? resolvedPath,
        [NotNullWhen(false)] out string? diagnosticCode,
        out string? diagnosticMessage)
    {
        resolvedPath = null;
        diagnosticCode = null;
        diagnosticMessage = null;

        if (string.IsNullOrWhiteSpace(rawPath))
        {
            diagnosticCode = "INVALID_REFERENCE_PATH";
            diagnosticMessage = "Reference path is empty or whitespace.";
            return false;
        }

        // 1. Remote URL policy: absolutely NO remote auto-downloads
        if (IsRemoteUrl(rawPath))
        {
            diagnosticCode = "XREF_REMOTE_NOT_SUPPORTED";
            diagnosticMessage = $"Remote URL references are not supported for local offline viewer: '{rawPath}'";
            return false;
        }

        var searchDirsList = searchDirectories?.Where(d => !string.IsNullOrWhiteSpace(d)).ToList() ?? [];
        if (searchDirsList.Count == 0)
        {
            diagnosticCode = "MISSING_EXTERNAL_REFERENCE";
            diagnosticMessage = $"No search directories provided to resolve reference '{rawPath}'.";
            return false;
        }

        // 2. Path traversal security check
        var normalizedRaw = rawPath.Replace('\\', '/');
        var fileName = Path.GetFileName(normalizedRaw);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            diagnosticCode = "INVALID_REFERENCE_PATH";
            diagnosticMessage = $"Cannot determine filename from reference path: '{rawPath}'.";
            return false;
        }

        foreach (var dir in searchDirsList)
        {
            if (!Directory.Exists(dir)) continue;

            var fullDirPath = Path.GetFullPath(dir);

            // Test 1: Relative subpath if rawPath is not rooted
            if (!Path.IsPathRooted(normalizedRaw) && normalizedRaw.Contains('/'))
            {
                var combined = Path.GetFullPath(Path.Combine(fullDirPath, normalizedRaw));
                // Guard: Must stay inside search directory
                if (!combined.StartsWith(fullDirPath, StringComparison.OrdinalIgnoreCase))
                {
                    diagnosticCode = "PATH_TRAVERSAL_PREVENTED";
                    diagnosticMessage = $"Path traversal attempt detected and blocked: '{rawPath}'.";
                    return false;
                }

                if (File.Exists(combined))
                {
                    resolvedPath = combined;
                    return true;
                }
            }

            // Test 2: Case-insensitive sibling lookup by filename
            try
            {
                var candidate = Directory.EnumerateFiles(fullDirPath, "*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(f => string.Equals(Path.GetFileName(f), fileName, StringComparison.OrdinalIgnoreCase));

                if (candidate != null)
                {
                    resolvedPath = Path.GetFullPath(candidate);
                    return true;
                }
            }
            catch (Exception ex)
            {
                diagnosticCode = "DIRECTORY_ENUMERATION_ERROR";
                diagnosticMessage = $"Failed to enumerate search directory '{dir}': {ex.Message}";
                return false;
            }
        }

        // Check if rawPath contained directory traversal characters that weren't found
        if (normalizedRaw.Contains("../") || normalizedRaw.Contains("/.."))
        {
            diagnosticCode = "PATH_TRAVERSAL_PREVENTED";
            diagnosticMessage = $"Reference path with traversal segments could not be resolved safely: '{rawPath}'.";
            return false;
        }

        diagnosticCode = "EXTERNAL_RESOURCE_NOT_FOUND";
        diagnosticMessage = $"External reference file '{fileName}' was not found in granted directories.";
        return false;
    }
}
