// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Base class for Material 3 compact action and selection chips. Renders a 32dp-high,
/// 8dp-rounded outlined container with an optional 18dp leading <see cref="Icon"/>.
/// Set <see cref="IsElevated"/> for the elevated container treatment (no outline,
/// SurfaceContainerLow background, level-1 shadow) used on visually busy surfaces.
/// </summary>
public class Chip : Button
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<Chip, object?>(nameof(Icon));

    public static readonly StyledProperty<bool> IsElevatedProperty =
        AvaloniaProperty.Register<Chip, bool>(nameof(IsElevated));

    /// <summary>Optional leading graphic, displayed at 18x18 before the label.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// When true, uses the elevated chip container (no outline, SurfaceContainerLow
    /// background and a level-1 shadow) instead of the default flat outlined container.
    /// </summary>
    public bool IsElevated
    {
        get => GetValue(IsElevatedProperty);
        set => SetValue(IsElevatedProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            PseudoClasses.Set(":with-icon", change.NewValue is not null);
        }
    }
}
