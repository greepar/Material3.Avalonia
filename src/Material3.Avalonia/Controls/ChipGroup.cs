// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 chip container: lays out chips in a wrapping flow with an 8dp gap
/// between chips and between wrapped lines (adjustable via <see cref="Spacing"/>).
/// </summary>
public class ChipGroup : ItemsControl
{
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<ChipGroup, double>(nameof(Spacing), 8.0);

    public ChipGroup()
    {
        ClipToBounds = false;
        ItemsPanel = new FuncTemplate<Panel?>(CreateItemsPanel);
    }

    /// <summary>Gap between adjacent chips and between wrapped lines. Defaults to 8.</summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    private Panel CreateItemsPanel()
    {
        var panel = new WrapPanel { ClipToBounds = false };
        panel.Bind(WrapPanel.ItemSpacingProperty, this.GetObservable(SpacingProperty));
        panel.Bind(WrapPanel.LineSpacingProperty, this.GetObservable(SpacingProperty));
        return panel;
    }
}
