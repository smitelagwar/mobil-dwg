using MobilDwg.Core.Diagnostics;

namespace MobilDwg.Core.Guards;

public sealed record CadResourceBudget
{
    public static CadResourceBudget Default { get; } = new();

    public long MaxFileSizeBytes { get; init; } = 256 * 1024 * 1024; // 256 MB
    public int MaxEntities { get; init; } = 250_000;
    public int MaxBlockNestingDepth { get; init; } = 32;
    public int MaxTextLength { get; init; } = 65_536; // 64 KB
    public int MaxHatchBoundarySegments { get; init; } = 10_000;
    public int MaxRasterDimensionPixels { get; init; } = 8_192; // 8K x 8K
    public long MaxRasterTotalPixels { get; init; } = 67_108_864; // 64 MP decompression bomb protection
    public int MaxXrefCount { get; init; } = 100;
    public double CoordinateMaxAbsoluteValue { get; init; } = 1e12; // Overflow threshold
}

public sealed class CadBudgetGuard
{
    private readonly CadResourceBudget _budget;
    private readonly List<CadDiagnostic> _diagnostics = new();

    public CadBudgetGuard(CadResourceBudget? budget = null)
    {
        _budget = budget ?? CadResourceBudget.Default;
    }

    public CadResourceBudget Budget => _budget;
    public IReadOnlyList<CadDiagnostic> Diagnostics => _diagnostics;

    public bool CheckFileSize(long sizeBytes, out CadDiagnostic? diagnostic)
    {
        if (sizeBytes > _budget.MaxFileSizeBytes)
        {
            diagnostic = new CadDiagnostic(
                "RESOURCE_BUDGET_EXCEEDED_FILE_SIZE",
                DiagnosticSeverity.Error,
                $"File size ({sizeBytes:N0} bytes) exceeds the safe resource budget limit of {_budget.MaxFileSizeBytes:N0} bytes.");
            _diagnostics.Add(diagnostic);
            return false;
        }

        diagnostic = null;
        return true;
    }

    public bool CheckEntityCount(int count, out CadDiagnostic? diagnostic)
    {
        if (count > _budget.MaxEntities)
        {
            diagnostic = new CadDiagnostic(
                "RESOURCE_BUDGET_EXCEEDED_ENTITIES",
                DiagnosticSeverity.Warning,
                $"Entity count ({count:N0}) reached the maximum allowed scene entity budget ({_budget.MaxEntities:N0}). Further entities will be truncated.");
            _diagnostics.Add(diagnostic);
            return false;
        }

        diagnostic = null;
        return true;
    }

    public bool CheckBlockDepth(int depth, out CadDiagnostic? diagnostic)
    {
        if (depth > _budget.MaxBlockNestingDepth)
        {
            diagnostic = new CadDiagnostic(
                "RESOURCE_BUDGET_EXCEEDED_BLOCK_DEPTH",
                DiagnosticSeverity.Warning,
                $"Block nesting depth ({depth}) exceeds maximum limit of {_budget.MaxBlockNestingDepth}. Recursion truncated to prevent stack overflow.");
            _diagnostics.Add(diagnostic);
            return false;
        }

        diagnostic = null;
        return true;
    }

    public bool CheckTextLength(int length, out CadDiagnostic? diagnostic)
    {
        if (length > _budget.MaxTextLength)
        {
            diagnostic = new CadDiagnostic(
                "RESOURCE_BUDGET_EXCEEDED_TEXT_LENGTH",
                DiagnosticSeverity.Warning,
                $"Text character length ({length:N0}) exceeds maximum allowed budget ({_budget.MaxTextLength:N0}). Text will be truncated.");
            _diagnostics.Add(diagnostic);
            return false;
        }

        diagnostic = null;
        return true;
    }

    public bool CheckHatchSegments(int segments, out CadDiagnostic? diagnostic)
    {
        if (segments > _budget.MaxHatchBoundarySegments)
        {
            diagnostic = new CadDiagnostic(
                "RESOURCE_BUDGET_EXCEEDED_HATCH_SEGMENTS",
                DiagnosticSeverity.Warning,
                $"Hatch boundary loop contains {segments:N0} segments, exceeding safe limit ({_budget.MaxHatchBoundarySegments:N0}). Complex fill suppressed.");
            _diagnostics.Add(diagnostic);
            return false;
        }

        diagnostic = null;
        return true;
    }

    public bool CheckRasterDimensions(int width, int height, out CadDiagnostic? diagnostic)
    {
        if (width <= 0 || height <= 0)
        {
            diagnostic = new CadDiagnostic(
                "RESOURCE_INVALID_RASTER_DIMENSIONS",
                DiagnosticSeverity.Warning,
                $"Raster dimensions ({width}x{height}) are degenerate or negative.");
            _diagnostics.Add(diagnostic);
            return false;
        }

        if (width > _budget.MaxRasterDimensionPixels || height > _budget.MaxRasterDimensionPixels)
        {
            diagnostic = new CadDiagnostic(
                "RESOURCE_BUDGET_EXCEEDED_RASTER_DIMENSIONS",
                DiagnosticSeverity.Warning,
                $"Raster dimension ({width}x{height}) exceeds maximum dimension of {_budget.MaxRasterDimensionPixels} px.");
            _diagnostics.Add(diagnostic);
            return false;
        }

        long totalPixels = (long)width * height;
        if (totalPixels > _budget.MaxRasterTotalPixels)
        {
            diagnostic = new CadDiagnostic(
                "RESOURCE_BUDGET_EXCEEDED_RASTER_PIXELS",
                DiagnosticSeverity.Warning,
                $"Raster total pixel count ({totalPixels:N0}) exceeds safe memory budget of {_budget.MaxRasterTotalPixels:N0} pixels (decompression bomb guard).");
            _diagnostics.Add(diagnostic);
            return false;
        }

        diagnostic = null;
        return true;
    }

    public bool CheckXrefCount(int count, out CadDiagnostic? diagnostic)
    {
        if (count > _budget.MaxXrefCount)
        {
            diagnostic = new CadDiagnostic(
                "RESOURCE_BUDGET_EXCEEDED_XREF_COUNT",
                DiagnosticSeverity.Warning,
                $"Drawing contains {count} external references, exceeding maximum allowed count ({_budget.MaxXrefCount}).");
            _diagnostics.Add(diagnostic);
            return false;
        }

        diagnostic = null;
        return true;
    }
}
