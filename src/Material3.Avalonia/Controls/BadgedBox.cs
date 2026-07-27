// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Hosts arbitrary content with a <see cref="Badge"/> (typically a
/// <see cref="Controls.Badge"/> control) anchored to the content's top-right corner.
/// </summary>
public class BadgedBox : ContentControl
{
    public static readonly StyledProperty<object?> BadgeProperty =
        AvaloniaProperty.Register<BadgedBox, object?>(nameof(Badge));

    /// <summary>The badge shown at the content's top-right corner, usually a <see cref="Controls.Badge"/>.</summary>
    public object? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }
}
