// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 rich tooltip container: a SurfaceContainer card with an optional subhead,
/// body content and an optional actions row. Place inside <c>ToolTip.Tip</c>.
/// </summary>
public class RichTooltip : ContentControl
{
    public static readonly StyledProperty<string?> SubheadProperty =
        AvaloniaProperty.Register<RichTooltip, string?>(nameof(Subhead));

    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<RichTooltip, object?>(nameof(Actions));

    /// <summary>Optional emphasized first line.</summary>
    public string? Subhead
    {
        get => GetValue(SubheadProperty);
        set => SetValue(SubheadProperty, value);
    }

    /// <summary>Optional actions row (typically text buttons).</summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }
}
