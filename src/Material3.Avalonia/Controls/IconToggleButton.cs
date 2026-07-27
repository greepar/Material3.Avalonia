// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls.Primitives;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 icon toggle button: a 40x40 icon button that toggles between selected and
/// unselected states. Unselected visuals match <see cref="IconButton"/> for the same
/// <see cref="Variant"/>; selecting animates the container color and morphs the shape
/// from fully rounded to a 12dp corner (M3 Expressive shape change).
/// </summary>
public class IconToggleButton : ToggleButton
{
    public static readonly StyledProperty<IconButtonVariant> VariantProperty =
        AvaloniaProperty.Register<IconToggleButton, IconButtonVariant>(nameof(Variant));

    /// <summary>The container/color treatment. Defaults to <see cref="IconButtonVariant.Standard"/>.</summary>
    public IconButtonVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }
}
