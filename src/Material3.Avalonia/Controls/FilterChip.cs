// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 filter chip: a selectable chip that toggles a persistent selected state
/// (inherited <see cref="ToggleButton.IsChecked"/>). When checked it uses the
/// SecondaryContainer colors, drops its outline and shows an animated leading checkmark.
/// An optional 18dp leading <see cref="Icon"/> is shown while unchecked.
/// </summary>
public class FilterChip : ToggleButton
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<FilterChip, object?>(nameof(Icon));

    /// <summary>Optional leading graphic, displayed at 18x18 before the label while unchecked.</summary>
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
