// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 extended floating action button: a 56dp-high, 16dp-rounded FAB with an
/// optional leading 24dp <see cref="Icon"/> and a text label (<c>Content</c>).
/// Uses the PrimaryContainer color scheme; rests at elevation 3 and raises to
/// elevation 4 on hover. Do not place inside ClipToBounds containers.
/// </summary>
public class ExtendedFloatingActionButton : Button
{
    public static readonly StyledProperty<FabColor> ColorProperty =
        FloatingActionButton.ColorProperty.AddOwner<ExtendedFloatingActionButton>();

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<ExtendedFloatingActionButton, object?>(nameof(Icon));

    /// <summary>The color scheme. Defaults to <see cref="FabColor.PrimaryContainer"/>.</summary>
    public FabColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Optional leading graphic, displayed at 24x24 before the label.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
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
