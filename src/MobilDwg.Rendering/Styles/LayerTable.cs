using System.Diagnostics.CodeAnalysis;

namespace MobilDwg.Rendering.Styles;

public sealed class LayerTable
{
    private readonly Dictionary<string, LayerDefinition> _layers = new(StringComparer.OrdinalIgnoreCase);

    public LayerTable()
    {
        // Standard Layer 0 always exists in any CAD drawing
        _layers["0"] = new LayerDefinition("0", CadColor.FromAci(7), CadLinetype.Continuous, CadLineweight.Default);
    }

    public LayerTable(IEnumerable<LayerDefinition> layers) : this()
    {
        ArgumentNullException.ThrowIfNull(layers);
        foreach (var layer in layers)
        {
            AddOrUpdate(layer);
        }
    }

    public IReadOnlyList<LayerDefinition> Layers => _layers.Values.OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    public void AddOrUpdate(LayerDefinition layer)
    {
        ArgumentNullException.ThrowIfNull(layer);
        _layers[layer.Name] = layer;
    }

    public bool TryGetLayer(string name, [NotNullWhen(true)] out LayerDefinition? layer)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            layer = null;
            return false;
        }

        return _layers.TryGetValue(name, out layer);
    }

    public LayerDefinition GetLayer(string name)
    {
        if (TryGetLayer(name, out var layer))
        {
            return layer;
        }

        return _layers["0"];
    }

    public bool SetLayerVisibility(string name, bool isVisible)
    {
        if (TryGetLayer(name, out var existing))
        {
            _layers[name] = existing with { IsVisible = isVisible };
            return true;
        }
        return false;
    }

    public bool SetLayerFrozen(string name, bool isFrozen)
    {
        if (TryGetLayer(name, out var existing))
        {
            _layers[name] = existing with { IsFrozen = isFrozen };
            return true;
        }
        return false;
    }

    public bool IsLayerVisible(string name)
    {
        return TryGetLayer(name, out var l) ? l.IsVisible && !l.IsFrozen : true;
    }
}
