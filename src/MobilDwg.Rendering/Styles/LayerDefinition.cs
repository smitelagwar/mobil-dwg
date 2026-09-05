namespace MobilDwg.Rendering.Styles;

public sealed record LayerDefinition
{
    public string Name { get; }
    public bool IsVisible { get; init; } = true;
    public bool IsFrozen { get; init; } = false;
    public bool IsLocked { get; init; } = false;
    public CadColor Color { get; init; }
    public CadLinetype Linetype { get; init; }
    public CadLineweight Lineweight { get; init; }

    public bool IsRenderable => IsVisible && !IsFrozen;

    public LayerDefinition(
        string name,
        CadColor? color = null,
        CadLinetype? linetype = null,
        CadLineweight? lineweight = null,
        bool isVisible = true,
        bool isFrozen = false,
        bool isLocked = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Layer name cannot be empty.", nameof(name));

        Name = name;
        Color = color ?? CadColor.FromAci(7);
        Linetype = linetype ?? CadLinetype.Continuous;
        Lineweight = lineweight ?? CadLineweight.Default;
        IsVisible = isVisible;
        IsFrozen = isFrozen;
        IsLocked = isLocked;
    }
}
