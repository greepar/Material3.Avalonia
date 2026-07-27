// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 scrim: a full-bleed Md3ScrimBrush overlay that fades to 32% opacity when
/// <see cref="IsOpen"/> is true. Pressing the scrim raises <see cref="Dismissed"/> so
/// the host can close the associated surface.
/// </summary>
public class Scrim : TemplatedControl
{
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Scrim, bool>(nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Whether the scrim is visible. Two-way bindable.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Raised when the user presses the scrim (light dismiss).</summary>
    public event EventHandler? Dismissed;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsOpen)
        {
            Dismissed?.Invoke(this, EventArgs.Empty);
        }
    }
}
