// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls.Primitives;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 bottom app bar: an 80dp-high SurfaceContainer bar hosting leading
/// <see cref="Actions"/> (typically icon buttons) and an optional trailing
/// <see cref="FloatingActionButton"/> slot.
/// </summary>
public class BottomAppBar : TemplatedControl
{
    public static readonly StyledProperty<object?> ActionsProperty =
        AvaloniaProperty.Register<BottomAppBar, object?>(nameof(Actions));

    public static readonly StyledProperty<object?> FloatingActionButtonProperty =
        AvaloniaProperty.Register<BottomAppBar, object?>(nameof(FloatingActionButton));

    /// <summary>Leading action content (typically a horizontal StackPanel of <see cref="IconButton"/>s).</summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    /// <summary>Optional floating action button displayed in the trailing slot.</summary>
    public object? FloatingActionButton
    {
        get => GetValue(FloatingActionButtonProperty);
        set => SetValue(FloatingActionButtonProperty, value);
    }
}
