// API modeled after m3fx (https://github.com/Glavo/m3fx), Apache-2.0; implementation follows the Material 3 spec.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace Material3.Avalonia.Controls;

/// <summary>
/// Material 3 modal bottom sheet: a bottom-anchored SurfaceContainerLow surface with
/// 28dp top corners, an optional drag handle and a light-dismiss scrim. Toggle with
/// <see cref="IsOpen"/>; <see cref="Closed"/> is raised when the sheet closes.
/// </summary>
[TemplatePart(PartScrim, typeof(Border))]
[TemplatePart(PartSheet, typeof(Border))]
[TemplatePart(PartDragHandle, typeof(Border))]
public class BottomSheet : ContentControl
{
    public const string PartScrim = "PART_Scrim";
    public const string PartSheet = "PART_Sheet";
    public const string PartDragHandle = "PART_DragHandle";

    private const double MinimumDismissDistance = 48;
    private const double MaximumDismissDistance = 96;

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<BottomSheet, bool>(nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> ShowDragHandleProperty =
        AvaloniaProperty.Register<BottomSheet, bool>(nameof(ShowDragHandle), true);

    private Border? _scrim;
    private Border? _sheet;
    private Border? _dragHandle;
    private double _dragStartY;
    private double _dragOffset;
    private bool _isDragging;

    public BottomSheet()
    {
        AddHandler(KeyDownEvent, OnSheetKeyDown, handledEventsToo: true);
    }

    /// <summary>Whether the sheet is open. Two-way bindable.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Whether the 32x4 drag handle is shown at the top of the sheet. Defaults to true.</summary>
    public bool ShowDragHandle
    {
        get => GetValue(ShowDragHandleProperty);
        set => SetValue(ShowDragHandleProperty, value);
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
        if (_dragHandle is not null)
        {
            _dragHandle.PointerPressed -= OnDragHandlePointerPressed;
            _dragHandle.PointerMoved -= OnDragHandlePointerMoved;
            _dragHandle.PointerReleased -= OnDragHandlePointerReleased;
            _dragHandle.PointerCaptureLost -= OnDragHandlePointerCaptureLost;
        }

        _scrim = e.NameScope.Find<Border>(PartScrim);
        _sheet = e.NameScope.Find<Border>(PartSheet);
        _dragHandle = e.NameScope.Find<Border>(PartDragHandle);
        if (_scrim is not null)
        {
            _scrim.PointerPressed += OnScrimPressed;
        }
        if (_dragHandle is not null)
        {
            _dragHandle.PointerPressed += OnDragHandlePointerPressed;
            _dragHandle.PointerMoved += OnDragHandlePointerMoved;
            _dragHandle.PointerReleased += OnDragHandlePointerReleased;
            _dragHandle.PointerCaptureLost += OnDragHandlePointerCaptureLost;
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

    private void OnDragHandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsOpen || _sheet is null || _dragHandle is null)
            return;

        var point = e.GetCurrentPoint(_dragHandle);
        if (e.Pointer.Type == PointerType.Mouse && !point.Properties.IsLeftButtonPressed)
            return;

        _dragStartY = e.GetPosition(this).Y;
        _dragOffset = 0;
        _isDragging = true;
        _sheet.Transitions = null;
        e.Pointer.Capture(_dragHandle);
        e.Handled = true;
    }

    private void OnDragHandlePointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || _sheet is null)
            return;

        _dragOffset = Math.Max(0, e.GetPosition(this).Y - _dragStartY);
        _sheet.RenderTransform = new TranslateTransform(0, _dragOffset);
        e.Handled = true;
    }

    private void OnDragHandlePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging)
            return;

        EndDrag(shouldDismiss: _sheet is not null && _dragOffset >= GetDismissDistance(_sheet.Bounds.Height));
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void OnDragHandlePointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_isDragging)
            EndDrag(shouldDismiss: false);
    }

    private void EndDrag(bool shouldDismiss)
    {
        _isDragging = false;
        _dragOffset = 0;

        if (_sheet is null)
            return;

        _sheet.ClearValue(TransitionsProperty);
        if (shouldDismiss)
            SetCurrentValue(IsOpenProperty, false);
        _sheet.ClearValue(RenderTransformProperty);
    }

    private static double GetDismissDistance(double sheetHeight) =>
        Math.Clamp(sheetHeight * 0.25, MinimumDismissDistance, MaximumDismissDistance);
}
