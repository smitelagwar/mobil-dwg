using System.Text.Json;

namespace MobilDwg.Core.Storage;

public sealed class RecentFilesManager
{
    public const int MaxCapacity = 10;
    private readonly List<RecentFileEntry> _entries = new();

    public IReadOnlyList<RecentFileEntry> Entries => _entries.AsReadOnly();

    public void AddOrPromote(RecentFileEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        // Remove existing if present (case-insensitive path comparison)
        _entries.RemoveAll(e => string.Equals(e.PathOrUri, entry.PathOrUri, StringComparison.OrdinalIgnoreCase));

        // Insert at the beginning (most recent)
        _entries.Insert(0, entry);

        // Enforce bounded capacity
        while (_entries.Count > MaxCapacity)
        {
            _entries.RemoveAt(_entries.Count - 1);
        }
    }

    public bool Remove(string pathOrUri)
    {
        if (string.IsNullOrWhiteSpace(pathOrUri)) return false;
        return _entries.RemoveAll(e => string.Equals(e.PathOrUri, pathOrUri, StringComparison.OrdinalIgnoreCase)) > 0;
    }

    public void Clear()
    {
        _entries.Clear();
    }

    public string SerializeJson()
    {
        return JsonSerializer.Serialize(_entries, new JsonSerializerOptions { WriteIndented = false });
    }

    public static RecentFilesManager DeserializeJson(string? json)
    {
        var manager = new RecentFilesManager();
        if (string.IsNullOrWhiteSpace(json))
        {
            return manager;
        }

        try
        {
            var list = JsonSerializer.Deserialize<List<RecentFileEntry>>(json);
            if (list != null)
            {
                // Add in reverse so earlier elements stay at the top
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    manager.AddOrPromote(list[i]);
                }
            }
        }
        catch
        {
            // Corrupt or invalid json yields clean manager
        }

        return manager;
    }
}
