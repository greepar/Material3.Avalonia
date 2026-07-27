// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 surface: a plain content container whose background and shadow follow the
/// M3 elevation scale. <see cref="Elevation"/> (0-5) maps to Surface,
/// SurfaceContainerLow, SurfaceContainer, SurfaceContainerHigh and
/// SurfaceContainerHighest with the corresponding box shadow.
/// </summary>
public class Surface : ContentControl
{
    public static readonly StyledProperty<int> ElevationProperty =
        AvaloniaProperty.Register<Surface, int>(nameof(Elevation), 0, coerce: CoerceElevation);

    private static int CoerceElevation(AvaloniaObject sender, int value)
        => Math.Clamp(value, 0, 5);

    /// <summary>Elevation level, 0-5. Defaults to 0.</summary>
    public int Elevation
    {
        get => GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }
}
