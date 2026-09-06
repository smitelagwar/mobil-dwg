using System.Collections.ObjectModel;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Spatial;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Scene;

public enum RenderBackgroundKind
{
    Dark = 0,
    Light = 1,
    Custom = 2,
}

public sealed record RenderColorContext(
    uint BackgroundArgb,
    uint DefaultForegroundArgb,
    RenderBackgroundKind BackgroundKind)
{
    public static RenderColorContext Dark { get; } = new(0xFF101010u, 0xFFF2F2F2u, RenderBackgroundKind.Dark);
    public static RenderColorContext Light { get; } = new(0xFFF8F8F8u, 0xFF111111u, RenderBackgroundKind.Light);
}

public sealed class RenderScene : IRenderScene
{
    private readonly ReadOnlyCollection<RenderSceneEntity> _entities;

    public RenderScene(
        IEnumerable<RenderSceneEntity> entities,
        SceneDiagnostics diagnostics,
        RenderColorContext colorContext,
        LayerTable? layerTable = null,
        StaticSceneBvh? spatialIndex = null)
    {
        ArgumentNullException.ThrowIfNull(entities);
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        ColorContext = colorContext ?? throw new ArgumentNullException(nameof(colorContext));
        LayerTable = layerTable ?? new LayerTable();

        if (spatialIndex != null && entities is ReadOnlyCollection<RenderSceneEntity> ro)
        {
            _entities = ro;
            WorldBounds = CalculateBounds(_entities);
            SpatialIndex = spatialIndex;
        }
        else
        {
            var sorted = entities
                .OrderBy(x => x.Source.SourceIndex ?? int.MaxValue)
                .ThenBy(x => x.Id.Value, StringComparer.Ordinal)
                .ToArray();
            _entities = Array.AsReadOnly(sorted);
            WorldBounds = CalculateBounds(_entities);
            SpatialIndex = spatialIndex ?? new StaticSceneBvh(_entities);
        }
    }

    public IReadOnlyList<RenderSceneEntity> Entities => _entities;
    public WorldBounds2? WorldBounds { get; }
    public SceneDiagnostics Diagnostics { get; }
    public RenderColorContext ColorContext { get; }
    public LayerTable LayerTable { get; }
    public StaticSceneBvh SpatialIndex { get; }

    private static WorldBounds2? CalculateBounds(IReadOnlyList<RenderSceneEntity> entities)
    {
        if (entities.Count == 0)
        {
            return null;
        }

        var bounds = entities[0].Bounds;
        for (var i = 1; i < entities.Count; i++)
        {
            bounds = bounds.Union(entities[i].Bounds);
        }

        return bounds;
    }
}

public sealed class RenderSceneAssembler
{
    private readonly List<RenderSceneEntity> _entities = new();
    private readonly List<SceneDiagnostic> _diagnostics = new();
    private readonly HashSet<string> _stableIds = new(StringComparer.Ordinal);
    private LayerTable _layerTable = new();

    public RenderSceneAssembler(RenderColorContext? colorContext = null)
    {
        ColorContext = colorContext ?? RenderColorContext.Dark;
    }

    public RenderColorContext ColorContext { get; }
    public LayerTable LayerTable => _layerTable;

    public void AddLayer(LayerDefinition layer)
    {
        ArgumentNullException.ThrowIfNull(layer);
        _layerTable.AddOrUpdate(layer);
    }

    public void SetLayerTable(LayerTable layerTable)
    {
        _layerTable = layerTable ?? throw new ArgumentNullException(nameof(layerTable));
    }

    public void AddEntity(RenderSceneEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (!_stableIds.Add(entity.Id.Value))
        {
            throw new InvalidOperationException($"Duplicate stable entity ID: {entity.Id.Value}");
        }

        _entities.Add(entity);
    }

    public void AddDiagnostic(SceneDiagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);
        _diagnostics.Add(diagnostic);
    }

    public RenderScene Build() => new(
        _entities,
        new SceneDiagnostics(_diagnostics),
        ColorContext,
        _layerTable);
}
