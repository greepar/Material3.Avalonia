// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 navigation rail: an 80dp-wide Surface column for primary destinations
/// in medium-width layouts. Destinations are <see cref="NavigationRailItem"/>s stacked
/// vertically; the optional <see cref="Header"/> slot at the top commonly hosts a
/// menu button or FAB. Single selection comes from the <see cref="ListBox"/> base.
/// </summary>
public class NavigationRail : ListBox
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<NavigationRail, object?>(nameof(Header));

    /// <summary>Optional top slot content (commonly a menu button or FAB).</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new NavigationRailItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<NavigationRailItem>(item, out recycleKey);
    }
}
