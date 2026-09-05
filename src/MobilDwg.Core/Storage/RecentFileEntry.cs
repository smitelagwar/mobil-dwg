using System.Text.Json.Serialization;

namespace MobilDwg.Core.Storage;

public sealed record RecentFileEntry
{
    [JsonPropertyName("name")]
    public string DisplayName { get; init; }

    [JsonPropertyName("path")]
    public string PathOrUri { get; init; }

    [JsonPropertyName("size")]
    public long SizeBytes { get; init; }

    [JsonPropertyName("time")]
    public DateTimeOffset LastOpenedUtc { get; init; }

    [JsonConstructor]
    public RecentFileEntry(string displayName, string pathOrUri, long sizeBytes, DateTimeOffset lastOpenedUtc)
    {
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Unnamed" : displayName;
        PathOrUri = pathOrUri ?? throw new ArgumentNullException(nameof(pathOrUri));
        SizeBytes = Math.Max(0, sizeBytes);
        LastOpenedUtc = lastOpenedUtc;
    }
}
