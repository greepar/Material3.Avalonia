// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 modal side sheet: a 256dp-wide, right-anchored SurfaceContainerLow surface
/// with 16dp leading corners and a light-dismiss scrim. Toggle with <see cref="IsOpen"/>;
/// <see cref="Closed"/> is raised when the sheet closes.
/// </summary>
[TemplatePart(PartScrim, typeof(Border))]
public class SideSheet : ContentControl
{
    public const string PartScrim = "PART_Scrim";

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<SideSheet, bool>(nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    private Border? _scrim;

    public SideSheet()
    {
        AddHandler(KeyDownEvent, OnSheetKeyDown, handledEventsToo: true);
    }

    /// <summary>Whether the sheet is open. Two-way bindable.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Raised when <see cref="IsOpen"/> transitions to false.</summary>
    public event EventHandler? Closed;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_scrim is not null)
        {
            _scrim.PointerPressed -= OnScrimPressed;
        }

        _scrim = e.NameScope.Find<Border>(PartScrim);
        if (_scrim is not null)
        {
            _scrim.PointerPressed += OnScrimPressed;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsOpenProperty && change.NewValue is false)
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border scrim
            && e.GetCurrentPoint(scrim).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
        {
            SetCurrentValue(IsOpenProperty, false);
            e.Handled = true;
        }
    }

    private void OnSheetKeyDown(object? sender, KeyEventArgs e)
    {
        if (IsOpen && e.Key == Key.Escape)
        {
            SetCurrentValue(IsOpenProperty, false);
            e.Handled = true;
        }
    }
}
