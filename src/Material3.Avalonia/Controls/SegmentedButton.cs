// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Material3.Avalonia.Controls;

/// <summary>
/// A single segment of a Material 3 segmented button set: a 40dp-high outlined toggle
/// with an optional 18dp leading <see cref="Icon"/>. When checked it shows the
/// SecondaryContainer treatment with an animated leading checkmark. Place segments
/// inside a <see cref="SegmentedButtonGroup"/>, which manages selection and the
/// first/last corner shape.
/// </summary>
public class SegmentedButton : ToggleButton
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SegmentedButton, object?>(nameof(Icon));

    /// <summary>Optional leading graphic, displayed at 18x18 before the label.</summary>
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
