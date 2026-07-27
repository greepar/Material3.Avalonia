// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 navigation drawer destination: a 56dp-high, fully rounded pill row with
/// a 24dp leading <see cref="Icon"/> slot, the label as <c>Content</c>, and an optional
/// trailing <see cref="BadgeText"/>. Selected rows use SecondaryContainer. Place these
/// inside an ordinary <see cref="ListBox"/> hosted in a SplitView pane to build a drawer.
/// </summary>
public class NavigationDrawerItem : ListBoxItem
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<NavigationDrawerItem, object?>(nameof(Icon));

    public static readonly StyledProperty<string?> BadgeTextProperty =
        AvaloniaProperty.Register<NavigationDrawerItem, string?>(nameof(BadgeText));

    /// <summary>The 24x24 leading icon.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Optional trailing badge label (for example an unread count).</summary>
    public string? BadgeText
    {
        get => GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            PseudoClasses.Set(":has-icon", change.NewValue is not null);
        }
        else if (change.Property == BadgeTextProperty)
        {
            PseudoClasses.Set(":has-badge", change.NewValue is not null);
        }
    }
}
