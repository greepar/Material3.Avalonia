// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>Container sizes for <see cref="FloatingActionButton"/>.</summary>
public enum FabSize
{
    /// <summary>40dp container, 12dp corners.</summary>
    Small,
    /// <summary>56dp container, 16dp corners (the default FAB).</summary>
    Medium,
    /// <summary>96dp container, 28dp corners.</summary>
    Large,
}

/// <summary>Color schemes for <see cref="FloatingActionButton"/>.</summary>
public enum FabColor
{
    /// <summary>PrimaryContainer background with OnPrimaryContainer icon (the default).</summary>
    PrimaryContainer,
    /// <summary>SurfaceContainerHigh background with Primary icon.</summary>
    Surface,
    /// <summary>SecondaryContainer background with OnSecondaryContainer icon.</summary>
    Secondary,
    /// <summary>TertiaryContainer background with OnTertiaryContainer icon.</summary>
    Tertiary,
}

/// <summary>
/// Material 3 floating action button: an elevated button for the most important
/// screen-level action. The icon is provided as <c>Content</c>. Rests at elevation 3
/// and raises to elevation 4 on hover. The shadow is drawn outside the control bounds;
/// do not place FABs inside ClipToBounds containers.
/// </summary>
public class FloatingActionButton : Button
{
    public static readonly StyledProperty<FabSize> SizeProperty =
        AvaloniaProperty.Register<FloatingActionButton, FabSize>(nameof(Size), FabSize.Medium);

    public static readonly StyledProperty<FabColor> ColorProperty =
        AvaloniaProperty.Register<FloatingActionButton, FabColor>(
            nameof(Color), FabColor.PrimaryContainer, inherits: true);

    /// <summary>The container size. Defaults to <see cref="FabSize.Medium"/> (56dp).</summary>
    public FabSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>The color scheme. Defaults to <see cref="FabColor.PrimaryContainer"/>.</summary>
    public FabColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }
}
