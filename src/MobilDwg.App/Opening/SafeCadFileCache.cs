using System.Buffers;

namespace MobilDwg.App.Opening;

public sealed class CachedCadFile : IAsyncDisposable
{
    private string? _filePath;

    internal CachedCadFile(string filePath, string displayName, long length)
    {
        _filePath = filePath;
        DisplayName = displayName;
        Length = length;
        SafeCadFileCache.RegisterActiveFile(filePath);
    }

    public string FilePath =>
        Volatile.Read(ref _filePath) ?? throw new ObjectDisposedException(nameof(CachedCadFile));

    public string DisplayName { get; }

    public long Length { get; }

    public Stream OpenRead()
    {
        return new FileStream(
            FilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 128 * 1024,
            options: FileOptions.SequentialScan);
    }

    public ValueTask DisposeAsync()
    {
        var filePath = Interlocked.Exchange(ref _filePath, null);
        if (filePath is not null)
        {
            SafeCadFileCache.UnregisterActiveFile(filePath);
            TryDelete(filePath);
        }

        return ValueTask.CompletedTask;
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (DirectoryNotFoundException)
        {
        }
        catch (FileNotFoundException)
        {
        }
    }
}

public sealed class SafeCadFileCache
{
    private const int BufferSize = 128 * 1024;
    private const long DiskRecheckIntervalBytes = 8L * 1024 * 1024;

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte> _activeFiles = new(StringComparer.OrdinalIgnoreCase);

    public static void RegisterActiveFile(string path)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            _activeFiles.TryAdd(Path.GetFullPath(path), 0);
        }
    }

    public static void UnregisterActiveFile(string path)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            _activeFiles.TryRemove(Path.GetFullPath(path), out _);
        }
    }

    public static bool IsFileActive(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        return _activeFiles.ContainsKey(Path.GetFullPath(path));
    }

    public static int ActiveFileCount => _activeFiles.Count;

    private readonly string _rootDirectory;
    private readonly CadFileOpenLimits _limits;
    private readonly Func<string, long> _availableBytesProvider;

    /// <summary>
    /// Purges orphaned temporary files in the private cache root directory.
    /// Does not delete files associated with currently active open leases.
    /// Safe to call from OnTrimMemory.
    /// </summary>
    public void PurgeOrphans()
    {
        if (!Directory.Exists(_rootDirectory))
        {
            return;
        }

        try
        {
            foreach (var file in Directory.EnumerateFiles(_rootDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                var full = Path.GetFullPath(file);
                if (!_activeFiles.ContainsKey(full))
                {
                    TryDelete(file);
                }
            }
        }
        catch (DirectoryNotFoundException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    /// <summary>
    /// Purges leftover orphaned files in the private cache root directory while protecting active files.
    /// </summary>
    public void PurgeAll()
    {
        PurgeOrphans();
    }

    public SafeCadFileCache(
        string rootDirectory,
        CadFileOpenLimits? limits = null,
        Func<string, long>? availableBytesProvider = null)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
        {
            throw new ArgumentException("A private cache root is required.", nameof(rootDirectory));
        }

        _rootDirectory = Path.GetFullPath(rootDirectory);
        _limits = limits ?? CadFileOpenLimits.Default;
        _availableBytesProvider = availableBytesProvider ?? GetAvailableBytes;
    }

    public string RootDirectory => _rootDirectory;

    public CadFileOpenLimits Limits => _limits;

    public async ValueTask<CachedCadFile> CopyAsync(
        CadFileSelection selection,
        long generation,
        IProgress<CadCacheCopyProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selection);
        cancellationToken.ThrowIfCancellationRequested();

        if (selection.DeclaredLength is > 0 && selection.DeclaredLength > _limits.MaxBytes)
        {
            throw new CadFileQuotaExceededException(selection.DeclaredLength.Value, _limits.MaxBytes);
        }

        Directory.CreateDirectory(_rootDirectory);
        EnsureFreeSpace();

        var displayName = SanitizeDisplayName(selection.DisplayName);
        var uniqueStem = $"g{generation:D10}-{Guid.NewGuid():N}";
        var finalPath = Path.Combine(_rootDirectory, $"{uniqueStem}-{displayName}");
        var temporaryPath = $"{finalPath}.part";
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        long total = 0;
        long lastDiskCheck = 0;

        RegisterActiveFile(temporaryPath);
        bool finalRegistered = false;

        try
        {
            await using var source = await selection.OpenReadAsync(cancellationToken).ConfigureAwait(false);
            if (!source.CanRead)
            {
                throw new IOException("Selected CAD source stream is not readable.");
            }

            await using (var destination = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: BufferSize,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                progress?.Report(CreateProgress(total, selection.DeclaredLength));

                while (true)
                {
                    var read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)
                        .ConfigureAwait(false);
                    if (read == 0)
                    {
                        break;
                    }

                    total = checked(total + read);
                    if (total > _limits.MaxBytes)
                    {
                        throw new CadFileQuotaExceededException(total, _limits.MaxBytes);
                    }

                    await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken)
                        .ConfigureAwait(false);

                    if (total - lastDiskCheck >= DiskRecheckIntervalBytes)
                    {
                        EnsureFreeSpace();
                        lastDiskCheck = total;
                    }

                    progress?.Report(CreateProgress(total, selection.DeclaredLength));
                }

                await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
                destination.Flush(flushToDisk: true);
            }

            cancellationToken.ThrowIfCancellationRequested();

            RegisterActiveFile(finalPath);
            finalRegistered = true;

            File.Move(temporaryPath, finalPath);
            UnregisterActiveFile(temporaryPath);

            progress?.Report(CreateProgress(total, selection.DeclaredLength));
            return new CachedCadFile(finalPath, displayName, total);
        }
        catch
        {
            UnregisterActiveFile(temporaryPath);
            if (finalRegistered)
            {
                UnregisterActiveFile(finalPath);
            }
            TryDelete(temporaryPath);
            TryDelete(finalPath);
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private CadCacheCopyProgress CreateProgress(long bytesCopied, long? declaredLength)
    {
        double? fraction = null;
        if (declaredLength is > 0 && declaredLength <= _limits.MaxBytes)
        {
            fraction = Math.Clamp((double)bytesCopied / declaredLength.Value, 0d, 1d);
        }

        return new CadCacheCopyProgress(bytesCopied, declaredLength, fraction);
    }

    private void EnsureFreeSpace()
    {
        var availableBytes = _availableBytesProvider(_rootDirectory);
        if (availableBytes <= _limits.ReserveFreeBytes)
        {
            throw new CadFileInsufficientSpaceException(availableBytes, _limits.ReserveFreeBytes);
        }
    }

    private static long GetAvailableBytes(string path)
    {
        try
        {
            var root = Path.GetPathRoot(Path.GetFullPath(path));
            if (string.IsNullOrWhiteSpace(root))
            {
                return long.MaxValue;
            }

            var drive = new DriveInfo(root);
            return drive.IsReady ? drive.AvailableFreeSpace : long.MaxValue;
        }
        catch
        {
            return long.MaxValue;
        }
    }

    internal static string SanitizeDisplayName(string? displayName)
    {
        var candidate = string.IsNullOrWhiteSpace(displayName) ? "drawing.cad" : displayName.Trim();
        candidate = candidate.Replace('\\', '/');
        var separator = candidate.LastIndexOf('/');
        if (separator >= 0)
        {
            candidate = candidate[(separator + 1)..];
        }

        if (string.IsNullOrWhiteSpace(candidate))
        {
            candidate = "drawing.cad";
        }

        var extension = Path.GetExtension(candidate);
        var safeExtension = extension.Equals(".dwg", StringComparison.OrdinalIgnoreCase)
            ? ".dwg"
            : extension.Equals(".dxf", StringComparison.OrdinalIgnoreCase)
                ? ".dxf"
                : ".cad";

        var baseName = Path.GetFileNameWithoutExtension(candidate);
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "drawing";
        }

        var characters = new char[Math.Min(baseName.Length, 80)];
        for (var index = 0; index < characters.Length; index++)
        {
            var character = baseName[index];
            characters[index] = char.IsLetterOrDigit(character) || character is ' ' or '_' or '-' or '(' or ')' or '[' or ']'
                ? character
                : '_';
        }

        var sanitizedBase = new string(characters).Trim(' ', '.');
        if (string.IsNullOrWhiteSpace(sanitizedBase))
        {
            sanitizedBase = "drawing";
        }

        return sanitizedBase + safeExtension;
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (DirectoryNotFoundException)
        {
        }
        catch (FileNotFoundException)
        {
        }
    }
}
