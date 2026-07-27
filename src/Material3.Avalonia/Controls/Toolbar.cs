// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace Material3.Avalonia.Controls;

/// <summary>Positioning variants for <see cref="Toolbar"/>.</summary>
public enum ToolbarVariant
{
    /// <summary>Full-width flat bar attached to an application edge (default).</summary>
    Docked,
    /// <summary>Fully rounded, elevated bar floating over application content.</summary>
    Floating,
}

/// <summary>
/// Material 3 (Expressive) toolbar: a 64dp-high SurfaceContainer bar hosting a row of
/// related actions supplied as <see cref="Content"/> (typically a horizontal StackPanel
/// of icon buttons). <see cref="ToolbarVariant.Docked"/> spans the full width;
/// <see cref="ToolbarVariant.Floating"/> uses a full corner radius, level-2 elevation
/// and an outer margin.
/// </summary>
public class Toolbar : TemplatedControl
{
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<Toolbar, object?>(nameof(Content));

    public static readonly StyledProperty<ToolbarVariant> VariantProperty =
        AvaloniaProperty.Register<Toolbar, ToolbarVariant>(nameof(Variant), ToolbarVariant.Docked);

    /// <summary>The toolbar content (typically a horizontal StackPanel of actions).</summary>
    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>The positioning variant. Defaults to <see cref="ToolbarVariant.Docked"/>.</summary>
    public ToolbarVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }
}
