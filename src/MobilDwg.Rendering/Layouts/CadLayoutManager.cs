using System.Collections.ObjectModel;
using MobilDwg.Rendering.Diagnostics;
using MobilDwg.Rendering.Geometry;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Styles;
using MobilDwg.Rendering.Transforms;

namespace MobilDwg.Rendering.Layouts;

public sealed class CadLayoutManager
{
    private readonly RenderScene _modelSpaceScene;
    private readonly Dictionary<string, CadLayoutDefinition> _layouts;
    private string _activeLayoutName;

    public CadLayoutManager(
        RenderScene modelSpaceScene,
        IEnumerable<CadLayoutDefinition>? layouts = null,
        string? activeLayoutName = null)
    {
        _modelSpaceScene = modelSpaceScene ?? throw new ArgumentNullException(nameof(modelSpaceScene));
        _layouts = new Dictionary<string, CadLayoutDefinition>(StringComparer.OrdinalIgnoreCase);

        // Ensure default Model layout exists if not provided
        var modelBounds = modelSpaceScene.WorldBounds ?? new WorldBounds2(0, 0, 100, 100);
        _layouts["Model"] = new CadLayoutDefinition("Model", isModelSpace: true, tabOrder: 0, paperBounds: modelBounds);

        if (layouts != null)
        {
            foreach (var l in layouts)
            {
                _layouts[l.Name] = l;
            }
        }

        _activeLayoutName = activeLayoutName != null && _layouts.ContainsKey(activeLayoutName)
            ? activeLayoutName
            : "Model";
    }

    public string ActiveLayoutName => _activeLayoutName;
    public CadLayoutDefinition ActiveLayout => _layouts[_activeLayoutName];
    public IReadOnlyCollection<CadLayoutDefinition> Layouts => _layouts.Values;
    public RenderScene ModelSpaceScene => _modelSpaceScene;

    /// <summary>
    /// Switches the active layout in memory without reparsing the CAD document.
    /// </summary>
    public void SwitchLayout(string layoutName)
    {
        ArgumentNullException.ThrowIfNull(layoutName);
        if (!_layouts.ContainsKey(layoutName))
        {
            throw new ArgumentException($"Layout '{layoutName}' is not defined.", nameof(layoutName));
        }

        _activeLayoutName = layoutName;
    }

    /// <summary>
    /// Composes the active layout's RenderScene in memory. Zero reparse required.
    /// </summary>
    public RenderScene ComposeActiveScene(
        RenderColorContext? colorContext = null,
        ICollection<SceneDiagnostic>? diagnostics = null)
    {
        var layout = ActiveLayout;

        // If Model Space, return direct Model Space scene
        if (layout.IsModelSpace)
        {
            return _modelSpaceScene;
        }

        // Paper Space Composition
        var effectiveColorContext = colorContext ?? RenderColorContext.Dark;
        var assembler = new RenderSceneAssembler(effectiveColorContext);
        assembler.SetLayerTable(_modelSpaceScene.LayerTable);

        // 1. Add paper-space entities (title block, sheet frame, annotations)
        foreach (var paperEntity in layout.PaperEntities)
        {
            assembler.AddEntity(paperEntity);
        }

        // 2. Compose each active viewport
        var vpCounter = 0;
        foreach (var viewport in layout.Viewports)
        {
            if (!viewport.IsActive) continue;

            // Degenerate checks
            if (!double.IsFinite(viewport.PaperCenter.X) || !double.IsFinite(viewport.PaperCenter.Y) ||
                !double.IsFinite(viewport.PaperWidth) || viewport.PaperWidth <= 0 ||
                !double.IsFinite(viewport.PaperHeight) || viewport.PaperHeight <= 0 ||
                !double.IsFinite(viewport.ViewCenter.X) || !double.IsFinite(viewport.ViewCenter.Y) ||
                !double.IsFinite(viewport.ViewHeight) || viewport.ViewHeight <= 0 ||
                !double.IsFinite(viewport.TwistAngleRadians))
            {
                diagnostics?.Add(new SceneDiagnostic(
                    SceneDiagnosticKind.Unsupported,
                    "INVALID_VIEWPORT_GEOMETRY",
                    $"Viewport '{viewport.ViewportId}' contains non-finite or degenerate dimensions; skipped.",
                    new RenderEntityId(viewport.ViewportId)));
                continue;
            }

            // Compute Model-to-Paper Viewport Transform:
            // scale = PaperHeight / ViewHeight
            // 1. Translate model by -ViewCenter
            // 2. Rotate by -TwistAngleRadians
            // 3. Scale by scale
            // 4. Translate to PaperCenter
            var scale = viewport.PaperHeight / viewport.ViewHeight;
            var tPaper = Transform2D.CreateTranslation(viewport.PaperCenter.X, viewport.PaperCenter.Y);
            var s = Transform2D.CreateScale(scale, scale);
            var r = Transform2D.CreateRotation(-viewport.TwistAngleRadians);
            var tView = Transform2D.CreateTranslation(-viewport.ViewCenter.X, -viewport.ViewCenter.Y);
            var transform = tPaper * s * r * tView;

            var transformedInnerPrimitives = new List<RenderGeometryPrimitive>();

            // Filter and transform model entities
            foreach (var modelEntity in _modelSpaceScene.Entities)
            {
                // Viewport layer override: if layer is frozen in this viewport, skip!
                if (viewport.FrozenLayers.Contains(modelEntity.Layer.Value))
                {
                    continue;
                }

                foreach (var prim in modelEntity.Geometry)
                {
                    try
                    {
                        var transformedPrim = PrimitiveTransformer.Transform(prim, transform);
                        transformedInnerPrimitives.Add(transformedPrim);
                    }
                    catch (Exception ex)
                    {
                        diagnostics?.Add(new SceneDiagnostic(
                            SceneDiagnosticKind.Unsupported,
                            "VIEWPORT_PRIMITIVE_TRANSFORM_FAIL",
                            $"Primitive transform failed: {ex.Message}",
                            modelEntity.Id));
                    }
                }
            }

            // Create ViewportPrimitive containing inner primitives and paper clipping bounds
            var vpPrimitive = new ViewportPrimitive(
                viewport.ViewportId,
                viewport.PaperBounds,
                transformedInnerPrimitives,
                viewport.ClipBoundary);

            // Create Viewport Entity
            var vpEntity = new RenderSceneEntity(
                new RenderEntityId($"VP-{viewport.ViewportId}-{++vpCounter:D3}"),
                new RenderLayerToken("0"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("VIEWPORT"),
                [vpPrimitive]);

            assembler.AddEntity(vpEntity);

            // Add Viewport Border Outline Entity on paper sheet
            var b = viewport.PaperBounds;
            var borderPrims = new List<RenderGeometryPrimitive>
            {
                new LinePrimitive(new WorldPoint2(b.MinX, b.MinY), new WorldPoint2(b.MaxX, b.MinY)),
                new LinePrimitive(new WorldPoint2(b.MaxX, b.MinY), new WorldPoint2(b.MaxX, b.MaxY)),
                new LinePrimitive(new WorldPoint2(b.MaxX, b.MaxY), new WorldPoint2(b.MinX, b.MaxY)),
                new LinePrimitive(new WorldPoint2(b.MinX, b.MaxY), new WorldPoint2(b.MinX, b.MinY))
            };

            var borderEntity = new RenderSceneEntity(
                new RenderEntityId($"VP-BORDER-{viewport.ViewportId}"),
                new RenderLayerToken("0"),
                new RenderStyleToken("BYLAYER"),
                new RenderSourceReference("VIEWPORT_BORDER"),
                borderPrims);

            assembler.AddEntity(borderEntity);
        }

        return assembler.Build();
    }
}
