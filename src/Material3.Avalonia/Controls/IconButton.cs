// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>Color/emphasis variants for <see cref="IconButton"/> and <see cref="IconToggleButton"/>.</summary>
public enum IconButtonVariant
{
    /// <summary>Transparent container with OnSurfaceVariant icon (lowest emphasis).</summary>
    Standard,
    /// <summary>Primary container with OnPrimary icon (highest emphasis).</summary>
    Filled,
    /// <summary>SecondaryContainer with OnSecondaryContainer icon (medium emphasis).</summary>
    Tonal,
    /// <summary>Transparent container with a 1px Outline border and OnSurfaceVariant icon.</summary>
    Outlined,
}

/// <summary>
/// Material 3 icon button: a 40x40 circular button for a compact icon-only action.
/// The icon is provided as <c>Content</c> (typically a 24x24 <c>PathIcon</c> or <c>Path</c>).
/// Choose the container treatment with <see cref="Variant"/>.
/// </summary>
public class IconButton : Button
{
    public static readonly StyledProperty<IconButtonVariant> VariantProperty =
        AvaloniaProperty.Register<IconButton, IconButtonVariant>(nameof(Variant));

    /// <summary>The container/color treatment. Defaults to <see cref="IconButtonVariant.Standard"/>.</summary>
    public IconButtonVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }
}
