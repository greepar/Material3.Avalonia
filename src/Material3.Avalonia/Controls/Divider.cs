// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 divider: a 1px OutlineVariant rule. Horizontal dividers stretch across the
/// available width; vertical dividers stretch across the available height.
/// </summary>
public class Divider : TemplatedControl
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Divider, Orientation>(nameof(Orientation), Orientation.Horizontal);

    /// <summary>The divider direction. Defaults to <see cref="Orientation.Horizontal"/>.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}
