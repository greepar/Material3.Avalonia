// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 navigation bar (bottom navigation): an 80dp-high SurfaceContainer bar
/// presenting three to five <see cref="NavigationBarItem"/> destinations, horizontally
/// equalized, with single selection managed by the <see cref="ListBox"/> base class.
/// </summary>
public class NavigationBar : ListBox
{
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new NavigationBarItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<NavigationBarItem>(item, out recycleKey);
    }
}
