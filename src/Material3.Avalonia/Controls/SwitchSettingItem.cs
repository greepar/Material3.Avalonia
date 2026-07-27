// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;

namespace Material3.Avalonia.Controls;

/// <summary>
/// A <see cref="SettingItem"/> whose trailing slot is a toggle switch; clicking anywhere
/// on the row toggles <see cref="IsChecked"/>.
/// </summary>
public class SwitchSettingItem : SettingItem
{
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<SwitchSettingItem, bool>(nameof(IsChecked),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Whether the switch is on.</summary>
    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>Raised whenever <see cref="IsChecked"/> changes.</summary>
    public event EventHandler? IsCheckedChanged;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsCheckedProperty)
            IsCheckedChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        // The embedded ToggleSwitch handles its own clicks; only toggle for row clicks.
        if (!e.Handled && e.InitialPressMouseButton == MouseButton.Left)
        {
            SetCurrentValue(IsCheckedProperty, !IsChecked);
            e.Handled = true;
        }
    }
}
