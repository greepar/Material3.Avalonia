// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Destination for a <see cref="NavigationBar"/>: a 24dp <see cref="Icon"/> inside a
/// 64x32 pill-shaped active indicator plus a label-medium <see cref="Label"/> beneath.
/// The selected pill uses SecondaryContainer with a scale/fade entrance animation.
/// </summary>
public class NavigationBarItem : ListBoxItem
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<NavigationBarItem, object?>(nameof(Icon));

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<NavigationBarItem, string?>(nameof(Label));

    /// <summary>The 24x24 destination icon.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>The label-medium destination label shown beneath the icon.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
}
